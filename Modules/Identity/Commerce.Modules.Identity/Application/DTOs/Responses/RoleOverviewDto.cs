namespace Commerce.Modules.Identity.Application.DTOs.Responses;

public sealed record RoleOverviewDto
{
    public string Code { get; init; } = default!;

    public string Name { get; init; } = default!;

    public bool IsActive { get; init; }

    public bool IsSystem { get; init; }
}
