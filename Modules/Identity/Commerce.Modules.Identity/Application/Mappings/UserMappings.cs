using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Domain;

namespace Commerce.Modules.Identity.Application.Mappings;

public static class UserMappings
{
    public static UserDto ToUserDto(this User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        return new UserDto(
            user.Id,
            user.UserName,
            user.NormalizedUserName,
            user.DisplayName,
            user.Email,
            user.IsActive,
            user.IsSystem,
            user.LastLoginAtUtc,
            user.CreatedAtUtc,
            user.CreatedBy,
            user.UpdatedAtUtc,
            user.UpdatedBy);
    }
}
