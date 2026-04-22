using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Domain;

namespace Commerce.Modules.Identity.Application.Mappings;

public static class AuthorizationMappings
{
    public static RoleOverviewDto ToRoleOverviewDto(this Role role)
    {
        ArgumentNullException.ThrowIfNull(role);

        return new RoleOverviewDto(
            role.Code,
            role.Name,
            role.IsActive,
            role.IsSystem);
    }

    public static PermissionOverviewDto ToPermissionOverviewDto(this Permission permission)
    {
        ArgumentNullException.ThrowIfNull(permission);

        return new PermissionOverviewDto(
            permission.Code,
            permission.Module,
            permission.Feature,
            permission.GroupName,
            permission.SortOrder);
    }
}
