using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Domain;
using Commerce.Modules.Identity.Infrastructure;
using Commerce.Modules.Identity.Infrastructure.Authentication;
using CommerceCore.Application.Abstractions;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Identity.Application.Commands.Login;

public sealed class LoginHandler : IRequestHandler<LoginCommand, ApiResponse<LoginResponse>>
{
    private readonly IdentityDbContext _dbContext;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public LoginHandler(
        IdentityDbContext dbContext,
        IPasswordHasherService passwordHasherService,
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService,
        IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _passwordHasherService = passwordHasherService ?? throw new ArgumentNullException(nameof(passwordHasherService));
        _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
        _refreshTokenService = refreshTokenService ?? throw new ArgumentNullException(nameof(refreshTokenService));
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    public async Task<ApiResponse<LoginResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var normalizedUserName = command.UserName.Trim().ToUpperInvariant();
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.NormalizedUserName == normalizedUserName, cancellationToken);

        if (user is null || !user.IsActive || string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            throw new UnauthorizedAppException("The provided username or password is invalid.");
        }

        if (!_passwordHasherService.Verify(user.PasswordHash, command.Password))
        {
            throw new UnauthorizedAppException("The provided username or password is invalid.");
        }

        var roles = await GetActiveRoleCodesAsync(user.Id, cancellationToken);
        var permissions = await GetEffectivePermissionCodesAsync(user.Id, cancellationToken);
        var expiresAtUtc = _jwtTokenService.GetExpirationUtc();
        var accessToken = _jwtTokenService.CreateToken(user, roles, permissions, expiresAtUtc);
        var refreshToken = _refreshTokenService.GenerateToken();
        var refreshExpiresAtUtc = _refreshTokenService.GetExpirationUtc();

        await _dbContext.UserSessions.AddAsync(new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RefreshTokenHash = _refreshTokenService.HashToken(refreshToken),
            ExpiresAtUtc = refreshExpiresAtUtc,
            LastUsedAtUtc = _dateTimeProvider.UtcNow
        }, cancellationToken);

        user.LastLoginAtUtc = _dateTimeProvider.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResponse<LoginResponse>
        {
            Status = StatusCodes.Status200OK,
            Data = new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                TokenType = "Bearer",
                ExpiresAtUtc = expiresAtUtc,
                RefreshExpiresAtUtc = refreshExpiresAtUtc
            }
        };
    }

    private async Task<List<string>> GetActiveRoleCodesAsync(Guid userId, CancellationToken cancellationToken)
    {
        var now = _dateTimeProvider.UtcNow;

        return await _dbContext.UserRoles
            .Where(x => x.UserId == userId)
            .Where(static x => x.IsActive)
            .Where(x => x.ExpiresAtUtc == null || x.ExpiresAtUtc > now)
            .Join(
                _dbContext.Roles.Where(static x => x.IsActive),
                userRole => userRole.RoleId,
                role => role.Id,
                static (_, role) => role.Code)
            .Distinct()
            .OrderBy(static x => x)
            .ToListAsync(cancellationToken);
    }

    private async Task<List<string>> GetEffectivePermissionCodesAsync(Guid userId, CancellationToken cancellationToken)
    {
        var now = _dateTimeProvider.UtcNow;

        var directPermissionRows = await _dbContext.UserPermissions
            .Where(x => x.UserId == userId)
            .Where(x => x.ExpiresAtUtc == null || x.ExpiresAtUtc > now)
            .Join(
                _dbContext.Permissions.Where(static x => x.IsActive),
                userPermission => userPermission.PermissionId,
                permission => permission.Id,
                static (userPermission, permission) => new { permission.Code, userPermission.Effect })
            .ToListAsync(cancellationToken);

        var activeRoleIds = await _dbContext.UserRoles
            .Where(x => x.UserId == userId)
            .Where(static x => x.IsActive)
            .Where(x => x.ExpiresAtUtc == null || x.ExpiresAtUtc > now)
            .Join(
                _dbContext.Roles.Where(static x => x.IsActive),
                userRole => userRole.RoleId,
                role => role.Id,
                static (userRole, _) => userRole.RoleId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var rolePermissionRows = activeRoleIds.Count == 0
            ? []
            : await _dbContext.RolePermissions
                .Where(x => activeRoleIds.Contains(x.RoleId))
                .Join(
                    _dbContext.Permissions.Where(static x => x.IsActive),
                    rolePermission => rolePermission.PermissionId,
                    permission => permission.Id,
                    static (rolePermission, permission) => new { permission.Code, rolePermission.Effect })
                .ToListAsync(cancellationToken);

        var permissionCodes = await _dbContext.Permissions
            .Where(static x => x.IsActive)
            .OrderBy(static x => x.SortOrder)
            .ThenBy(static x => x.Code)
            .Select(static x => x.Code)
            .ToListAsync(cancellationToken);

        var directEffects = directPermissionRows
            .GroupBy(static x => x.Code)
            .ToDictionary(static x => x.Key, static x => x.Select(static y => y.Effect).ToList());

        var roleEffects = rolePermissionRows
            .GroupBy(static x => x.Code)
            .ToDictionary(static x => x.Key, static x => x.Select(static y => y.Effect).ToList());

        var effectivePermissions = new List<string>();

        foreach (var code in permissionCodes)
        {
            if (directEffects.TryGetValue(code, out var userEffects))
            {
                if (userEffects.Contains(PermissionEffect.Deny))
                {
                    continue;
                }

                if (userEffects.Contains(PermissionEffect.Allow))
                {
                    effectivePermissions.Add(code);
                    continue;
                }
            }

            if (!roleEffects.TryGetValue(code, out var roleEffectsForCode))
            {
                continue;
            }

            if (roleEffectsForCode.Contains(PermissionEffect.Deny))
            {
                continue;
            }

            if (roleEffectsForCode.Contains(PermissionEffect.Allow))
            {
                effectivePermissions.Add(code);
            }
        }

        return effectivePermissions;
    }
}
