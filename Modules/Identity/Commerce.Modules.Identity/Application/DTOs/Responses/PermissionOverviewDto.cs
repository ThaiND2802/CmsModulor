namespace Commerce.Modules.Identity.Application.DTOs.Responses;

public sealed record PermissionOverviewDto
{
    public string Code { get; init; } = default!;

    public string Module { get; init; } = default!;

    public string? Feature { get; init; }

    public string GroupName { get; init; } = default!;

    public int SortOrder { get; init; }
}
