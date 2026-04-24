using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Domain;
using Commerce.Modules.Identity.Infrastructure;
using CommerceCore.Application.Abstractions;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Identity.Application.Commands.SyncRolePermissions;

public sealed class SyncRolePermissionsHandler : IRequestHandler<SyncRolePermissionsCommand, ApiResponse<RolePermissionAssignmentsResponse>>
{
    private readonly IdentityDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICurrentUser _currentUser;

    public SyncRolePermissionsHandler(
        IdentityDbContext dbContext,
        IDateTimeProvider dateTimeProvider,
        ICurrentUser currentUser)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<ApiResponse<RolePermissionAssignmentsResponse>> Handle(SyncRolePermissionsCommand command, CancellationToken cancellationToken)
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

        var normalizedItems = command.Permissions
            .Select(x => new
            {
                x.PermissionId,
                Effect = ParseEffect(x.Effect)
            })
            .ToList();

        var role = await _dbContext.Roles
            .FirstOrDefaultAsync(x => x.Id == command.RoleId, cancellationToken)
            ?? throw new NotFoundAppException($"Role '{command.RoleId}' was not found.");

        if (!role.IsActive)
        {
            throw new ValidationAppException("Inactive roles cannot receive permissions.");
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

        var existingAssignments = await _dbContext.RolePermissions
            .IgnoreQueryFilters()
            .Where(x => x.RoleId == command.RoleId)
            .ToListAsync(cancellationToken);

        var requestedItemsByPermissionId = normalizedItems.ToDictionary(static x => x.PermissionId);
        var now = _dateTimeProvider.UtcNow;
        var actor = string.IsNullOrWhiteSpace(_currentUser.UserId) ? _currentUser.UserName : _currentUser.UserId;

        foreach (var assignment in existingAssignments)
        {
            if (requestedItemsByPermissionId.TryGetValue(assignment.PermissionId, out var requestedItem))
            {
                assignment.Effect = requestedItem.Effect;
                assignment.AssignedAtUtc = now;
                assignment.AssignedBy = actor;
                assignment.IsDeleted = false;
                assignment.DeletedAtUtc = null;
                assignment.DeletedBy = null;
                continue;
            }

            if (!assignment.IsDeleted)
            {
                _dbContext.RolePermissions.Remove(assignment);
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

            await _dbContext.RolePermissions.AddAsync(new RolePermission
            {
                Id = Guid.NewGuid(),
                RoleId = command.RoleId,
                PermissionId = item.PermissionId,
                Effect = item.Effect,
                AssignedAtUtc = now,
                AssignedBy = actor
            }, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var syncedPermissions = await _dbContext.RolePermissions
            .Where(x => x.RoleId == role.Id)
            .Include(x => x.Permission)
            .OrderBy(x => x.Permission.SortOrder)
            .ThenBy(x => x.Permission.Code)
            .Select(x => new RolePermissionAssignmentDto
            {
                PermissionId = x.PermissionId,
                PermissionCode = x.Permission.Code,
                PermissionName = x.Permission.Name,
                Module = x.Permission.Module,
                Feature = x.Permission.Feature,
                GroupName = x.Permission.GroupName,
                Effect = x.Effect.ToString(),
                AssignedAtUtc = x.AssignedAtUtc,
                AssignedBy = x.AssignedBy
            })
            .ToListAsync(cancellationToken);

        return new ApiResponse<RolePermissionAssignmentsResponse>
        {
            Status = StatusCodes.Status200OK,
            Data = new RolePermissionAssignmentsResponse
            {
                RoleId = role.Id,
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
}
