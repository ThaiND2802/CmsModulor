using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Domain;

namespace Commerce.Modules.Identity.Application.Mappings;

public static class UserMappings
{
    public static UserDto ToUserDto(this User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            NormalizedUserName = user.NormalizedUserName,
            DisplayName = user.DisplayName,
            Email = user.Email,
            IsActive = user.IsActive,
            IsSystem = user.IsSystem,
            LastLoginAtUtc = user.LastLoginAtUtc,
            CreatedAtUtc = user.CreatedAtUtc,
            CreatedBy = user.CreatedBy,
            UpdatedAtUtc = user.UpdatedAtUtc,
            UpdatedBy = user.UpdatedBy
        };
    }
}
