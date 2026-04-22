using Commerce.Modules.Identity.Domain;
using CommerceCore.FeatureManagement.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Identity.Infrastructure.Security;

public sealed class DbPermissionGate : IPermissionGate
{
    private readonly IdentityDbContext _dbContext;

    public DbPermissionGate(IdentityDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public bool HasPermission(string userId, string permission)
    {
        return HasPermissionAsync(userId, permission).GetAwaiter().GetResult();
    }

    public async Task<bool> HasPermissionAsync(
        string userId,
        string permission,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(permission);

        var userLookupValue = userId.Trim();
        var permissionCode = permission.Trim().ToUpperInvariant();
        var now = DateTime.UtcNow;

        var userRecord = Guid.TryParse(userLookupValue, out var parsedUserId)
            ? await _dbContext.Users
                .Where(static x => x.IsActive)
                .Where(x => x.Id == parsedUserId)
                .Select(static x => new { x.Id })
                .FirstOrDefaultAsync(cancellationToken)
            : await _dbContext.Users
                .Where(static x => x.IsActive)
                .Where(x => x.NormalizedUserName == userLookupValue.ToUpperInvariant())
                .Select(static x => new { x.Id })
                .FirstOrDefaultAsync(cancellationToken);

        if (userRecord is null)
        {
            return false;
        }

        var permissionRecord = await _dbContext.Permissions
            .Where(static x => x.IsActive)
            .Where(x => x.Code == permissionCode)
            .Select(static x => new { x.Id })
            .FirstOrDefaultAsync(cancellationToken);

        if (permissionRecord is null)
        {
            return false;
        }

        var userEffects = await _dbContext.UserPermissions
            .Where(x => x.UserId == userRecord.Id && x.PermissionId == permissionRecord.Id)
            .Where(x => x.ExpiresAtUtc == null || x.ExpiresAtUtc > now)
            .Join(
                _dbContext.Permissions.Where(static x => x.IsActive),
                userPermission => userPermission.PermissionId,
                permissionEntity => permissionEntity.Id,
                static (userPermission, _) => userPermission.Effect)
            .ToListAsync(cancellationToken);

        if (userEffects.Contains(PermissionEffect.Deny))
        {
            return false;
        }

        if (userEffects.Contains(PermissionEffect.Allow))
        {
            return true;
        }

        var activeRoleIds = await _dbContext.UserRoles
            .Where(x => x.UserId == userRecord.Id)
            .Where(static x => x.IsActive)
            .Where(x => x.ExpiresAtUtc == null || x.ExpiresAtUtc > now)
            .Join(
                _dbContext.Roles.Where(static x => x.IsActive),
                userRole => userRole.RoleId,
                role => role.Id,
                static (userRole, _) => userRole.RoleId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (activeRoleIds.Count == 0)
        {
            return false;
        }

        var roleEffects = await _dbContext.RolePermissions
            .Where(x => activeRoleIds.Contains(x.RoleId) && x.PermissionId == permissionRecord.Id)
            .Join(
                _dbContext.Permissions.Where(static x => x.IsActive),
                rolePermission => rolePermission.PermissionId,
                permissionEntity => permissionEntity.Id,
                static (rolePermission, _) => rolePermission.Effect)
            .ToListAsync(cancellationToken);

        if (roleEffects.Contains(PermissionEffect.Deny))
        {
            return false;
        }

        if (roleEffects.Contains(PermissionEffect.Allow))
        {
            return true;
        }

        return false;
    }
}
