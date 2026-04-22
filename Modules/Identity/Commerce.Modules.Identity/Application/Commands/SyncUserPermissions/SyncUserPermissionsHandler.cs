using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Domain;
using Commerce.Modules.Identity.Infrastructure;
using CommerceCore.Application.Abstractions;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Identity.Application.Commands.SyncUserPermissions;

public sealed class SyncUserPermissionsHandler : IRequestHandler<SyncUserPermissionsCommand, ApiResponse<UserPermissionAssignmentsResponse>>
{
    private readonly IdentityDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICurrentUser _currentUser;

    public SyncUserPermissionsHandler(
        IdentityDbContext dbContext,
        IDateTimeProvider dateTimeProvider,
        ICurrentUser currentUser)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<ApiResponse<UserPermissionAssignmentsResponse>> Handle(SyncUserPermissionsCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var duplicatePermissionIds = command.Permissions
            .GroupBy(x => x.PermissionId)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToList();
        if (duplicatePermissionIds.Count != 0)
        {
            throw new ValidationAppException("Duplicate permission ids are not allowed.");
        }

        var now = _dateTimeProvider.UtcNow;
        var normalizedItems = command.Permissions
            .Select(x => new
            {
                x.PermissionId,
                Effect = ParseEffect(x.Effect),
                ExpiresAtUtc = NormalizeExpiry(x.ExpiresAtUtc, now)
            })
            .ToList();

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == command.UserId, cancellationToken)
            ?? throw new NotFoundAppException($"User '{command.UserId}' was not found.");

        if (!user.IsActive)
        {
            throw new ValidationAppException("Inactive users cannot receive direct permissions.");
        }

        if (Guid.TryParse(_currentUser.UserId, out var actorUserId) && actorUserId == command.UserId)
        {
            throw new ForbiddenAppException("Users cannot directly modify their own permission assignments.");
        }

        var requestedPermissionIds = normalizedItems
            .Select(static x => x.PermissionId)
            .ToList();

        var permissions = requestedPermissionIds.Count == 0
            ? []
            : await _dbContext.Permissions
                .Where(x => requestedPermissionIds.Contains(x.Id))
                .ToListAsync(cancellationToken);

        if (permissions.Count != requestedPermissionIds.Count)
        {
            throw new ValidationAppException("One or more permissions were not found.");
        }

        if (permissions.Any(static x => !x.IsActive))
        {
            throw new ValidationAppException("Inactive permissions cannot be assigned.");
        }

        var existingAssignments = await _dbContext.UserPermissions
            .IgnoreQueryFilters()
            .Where(x => x.UserId == command.UserId)
            .ToListAsync(cancellationToken);

        var requestedItemsByPermissionId = normalizedItems.ToDictionary(static x => x.PermissionId);
        var actor = string.IsNullOrWhiteSpace(_currentUser.UserId) ? _currentUser.UserName : _currentUser.UserId;

        if (Guid.TryParse(_currentUser.UserId, out actorUserId))
        {
            var requestedPermissionCodes = permissions
                .Where(x => requestedItemsByPermissionId.ContainsKey(x.Id))
                .Select(x => x.Code)
                .ToList();

            var actorAllowedPermissionCodes = await _dbContext.UserPermissions
                .Where(x => x.UserId == actorUserId && x.Effect == PermissionEffect.Allow)
                .Where(x => x.ExpiresAtUtc == null || x.ExpiresAtUtc > now)
                .Join(
                    _dbContext.Permissions.Where(static x => x.IsActive),
                    userPermission => userPermission.PermissionId,
                    permission => permission.Id,
                    static (userPermission, permission) => permission.Code)
                .ToListAsync(cancellationToken);

            var actorAllowedPermissionSet = actorAllowedPermissionCodes.ToHashSet(StringComparer.Ordinal);
            var forbiddenRequestedPermissionCode = requestedPermissionCodes
                .FirstOrDefault(code => !actorAllowedPermissionSet.Contains(code));
            if (forbiddenRequestedPermissionCode is not null)
            {
                throw new ForbiddenAppException($"The current user cannot assign permission '{forbiddenRequestedPermissionCode}'.");
            }
        }

        foreach (var assignment in existingAssignments)
        {
            if (requestedItemsByPermissionId.TryGetValue(assignment.PermissionId, out var requestedItem))
            {
                assignment.Effect = requestedItem.Effect;
                assignment.ExpiresAtUtc = requestedItem.ExpiresAtUtc;
                assignment.AssignedAtUtc = now;
                assignment.AssignedBy = actor;
                assignment.IsDeleted = false;
                assignment.DeletedAtUtc = null;
                assignment.DeletedBy = null;
                continue;
            }

            if (!assignment.IsDeleted)
            {
                _dbContext.UserPermissions.Remove(assignment);
            }
        }

        var existingPermissionIds = existingAssignments
            .Select(static x => x.PermissionId)
            .ToHashSet();

        foreach (var item in normalizedItems)
        {
            if (existingPermissionIds.Contains(item.PermissionId))
            {
                continue;
            }

            await _dbContext.UserPermissions.AddAsync(new UserPermission
            {
                Id = Guid.NewGuid(),
                UserId = command.UserId,
                PermissionId = item.PermissionId,
                Effect = item.Effect,
                AssignedAtUtc = now,
                AssignedBy = actor,
                ExpiresAtUtc = item.ExpiresAtUtc
            }, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var syncedPermissions = await _dbContext.UserPermissions
            .Where(x => x.UserId == user.Id)
            .Include(x => x.Permission)
            .OrderBy(x => x.Permission.SortOrder)
            .ThenBy(x => x.Permission.Code)
            .Select(x => new UserPermissionAssignmentDto
            {
                PermissionId = x.PermissionId,
                PermissionCode = x.Permission.Code,
                PermissionName = x.Permission.Name,
                Module = x.Permission.Module,
                Feature = x.Permission.Feature,
                GroupName = x.Permission.GroupName,
                Effect = x.Effect.ToString(),
                AssignedAtUtc = x.AssignedAtUtc,
                AssignedBy = x.AssignedBy,
                ExpiresAtUtc = x.ExpiresAtUtc
            })
            .ToListAsync(cancellationToken);

        return new ApiResponse<UserPermissionAssignmentsResponse>
        {
            Status = StatusCodes.Status200OK,
            Data = new UserPermissionAssignmentsResponse
            {
                UserId = user.Id,
                Permissions = syncedPermissions
            }
        };
    }

    private static PermissionEffect ParseEffect(string effect)
    {
        return effect.Trim().ToUpperInvariant() switch
        {
            "ALLOW" => PermissionEffect.Allow,
            "DENY" => PermissionEffect.Deny,
            _ => throw new ValidationAppException($"Permission effect '{effect}' is invalid.")
        };
    }

    private static DateTime? NormalizeExpiry(DateTime? expiresAtUtc, DateTime now)
    {
        if (!expiresAtUtc.HasValue)
        {
            return null;
        }

        if (expiresAtUtc.Value.Kind != DateTimeKind.Utc)
        {
            throw new ValidationAppException("Permission expiry must be provided in UTC.");
        }

        if (expiresAtUtc.Value <= now)
        {
            throw new ValidationAppException("Permission expiry must be greater than the current time.");
        }

        return expiresAtUtc;
    }
}
