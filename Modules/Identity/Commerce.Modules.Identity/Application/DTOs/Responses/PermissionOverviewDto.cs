namespace Commerce.Modules.Identity.Application.DTOs.Responses;

public sealed record PermissionOverviewDto(
    string Code,
    string Module,
    string? Feature,
    string GroupName,
    int SortOrder);
