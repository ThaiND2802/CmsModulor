namespace Commerce.Modules.Identity.Application.DTOs.Responses;

public sealed record RoleOverviewDto(
    string Code,
    string Name,
    bool IsActive,
    bool IsSystem);
