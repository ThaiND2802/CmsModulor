namespace Commerce.Modules.Identity.Application.DTOs.Responses;

public sealed record AuthorizationOverviewDto
{
    public bool ModuleGatingRunsFirst { get; init; }

    public bool FeatureGatingRunsBeforeAuthorization { get; init; }

    public bool DbPermissionGateImplemented { get; init; }

    public string PermissionSource { get; init; } = default!;

    public string CurrentDevelopmentUserLookup { get; init; } = default!;

    public IReadOnlyCollection<string> ResolutionOrder { get; init; } = [];

    public IReadOnlyCollection<RoleOverviewDto> Roles { get; init; } = [];

    public IReadOnlyCollection<PermissionOverviewDto> Permissions { get; init; } = [];
}
