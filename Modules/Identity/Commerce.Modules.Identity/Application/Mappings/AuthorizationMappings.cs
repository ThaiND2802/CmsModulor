using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Domain;

namespace Commerce.Modules.Identity.Application.Mappings;

public static class AuthorizationMappings
{
    public static PermissionDto ToPermissionDto(this Permission permission)
    {
        ArgumentNullException.ThrowIfNull(permission);

        return new PermissionDto
        {
            Id = permission.Id,
            Code = permission.Code,
            Name = permission.Name,
            Description = permission.Description,
            Module = permission.Module,
            Feature = permission.Feature,
            GroupName = permission.GroupName,
            SortOrder = permission.SortOrder,
            IsActive = permission.IsActive,
            IsSystem = permission.IsSystem,
            CreatedAtUtc = permission.CreatedAtUtc,
            CreatedBy = permission.CreatedBy,
            UpdatedAtUtc = permission.UpdatedAtUtc,
            UpdatedBy = permission.UpdatedBy
        };
    }

    public static RoleDto ToRoleDto(this Role role)
    {
        ArgumentNullException.ThrowIfNull(role);

        return new RoleDto
        {
            Id = role.Id,
            Code = role.Code,
            Name = role.Name,
            Description = role.Description,
            IsActive = role.IsActive,
            IsSystem = role.IsSystem,
            CreatedAtUtc = role.CreatedAtUtc,
            CreatedBy = role.CreatedBy,
            UpdatedAtUtc = role.UpdatedAtUtc,
            UpdatedBy = role.UpdatedBy
        };
    }

    public static RoleOverviewDto ToRoleOverviewDto(this Role role)
    {
        ArgumentNullException.ThrowIfNull(role);

        return new RoleOverviewDto
        {
            Code = role.Code,
            Name = role.Name,
            IsActive = role.IsActive,
            IsSystem = role.IsSystem
        };
    }

    public static PermissionOverviewDto ToPermissionOverviewDto(this Permission permission)
    {
        ArgumentNullException.ThrowIfNull(permission);

        return new PermissionOverviewDto
        {
            Code = permission.Code,
            Module = permission.Module,
            Feature = permission.Feature,
            GroupName = permission.GroupName,
            SortOrder = permission.SortOrder
        };
    }
}
