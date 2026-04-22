namespace Commerce.Modules.Identity.Application.DTOs.Responses;

public sealed record UserDto(
    Guid Id,
    string UserName,
    string NormalizedUserName,
    string? DisplayName,
    string? Email,
    bool IsActive,
    bool IsSystem,
    DateTime? LastLoginAtUtc,
    DateTime CreatedAtUtc,
    string? CreatedBy,
    DateTime? UpdatedAtUtc,
    string? UpdatedBy);
