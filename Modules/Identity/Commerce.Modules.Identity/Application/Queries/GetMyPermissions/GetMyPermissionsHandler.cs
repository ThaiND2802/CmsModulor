using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Infrastructure;
using Commerce.Modules.Identity.Infrastructure.Persistence.Seeds;
using CommerceCore.Application.Abstractions;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Identity.Application.Queries.GetMyPermissions;

public sealed class GetMyPermissionsHandler : IRequestHandler<GetMyPermissionsQuery, ApiResponse<MyPermissionsResponse>>
{
    private readonly IdentityDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public GetMyPermissionsHandler(IdentityDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<ApiResponse<MyPermissionsResponse>> Handle(GetMyPermissionsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (!_currentUser.IsAuthenticated || !Guid.TryParse(_currentUser.UserId, out var userId))
        {
            throw new UnauthorizedAppException("The current user is not authenticated.");
        }

        var user = await _dbContext.Users
            .Where(static x => x.IsActive)
            .Select(x => new { x.Id, x.UserName })
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken)
            ?? throw new NotFoundAppException("The current user was not found.");

        var now = DateTime.UtcNow;

        var directPermissionRows = await _dbContext.UserPermissions
            .Where(x => x.UserId == user.Id)
            .Where(x => x.ExpiresAtUtc == null || x.ExpiresAtUtc > now)
            .Join(
                _dbContext.Permissions.Where(static x => x.IsActive),
                userPermission => userPermission.PermissionId,
                permission => permission.Id,
                static (userPermission, permission) => new { permission.Code, userPermission.Effect })
            .ToListAsync(cancellationToken);

        var activeRoleIds = await _dbContext.UserRoles
            .Where(x => x.UserId == user.Id)
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

        var permissionCodes = _dbContext.Permissions
            .Where(static x => x.IsActive)
            .OrderBy(static x => x.SortOrder)
            .ThenBy(static x => x.Code)
            .Select(static x => x.Code);

        var directEffects = directPermissionRows
            .GroupBy(static x => x.Code)
            .ToDictionary(static x => x.Key, static x => x.Select(static y => y.Effect).ToList());

        var roleEffects = rolePermissionRows
            .GroupBy(static x => x.Code)
            .ToDictionary(static x => x.Key, static x => x.Select(static y => y.Effect).ToList());

        var effectivePermissions = new List<string>();

        await foreach (var code in permissionCodes.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            if (directEffects.TryGetValue(code, out var userEffects))
            {
                if (userEffects.Contains(Domain.PermissionEffect.Deny))
                {
                    continue;
                }

                if (userEffects.Contains(Domain.PermissionEffect.Allow))
                {
                    effectivePermissions.Add(code);
                    continue;
                }
            }

            if (roleEffects.TryGetValue(code, out var roleEffectsForCode))
            {
                if (roleEffectsForCode.Contains(Domain.PermissionEffect.Deny))
                {
                    continue;
                }

                if (roleEffectsForCode.Contains(Domain.PermissionEffect.Allow))
                {
                    effectivePermissions.Add(code);
                }
            }
        }

        return new ApiResponse<MyPermissionsResponse>
        {
            Status = StatusCodes.Status200OK,
            Data = new MyPermissionsResponse
            {
                UserId = user.Id,
                UserName = user.UserName,
                IsAuthenticated = true,
                Permissions = effectivePermissions
            }
        };
    }
}
