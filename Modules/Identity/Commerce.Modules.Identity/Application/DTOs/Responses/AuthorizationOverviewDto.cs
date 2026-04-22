namespace Commerce.Modules.Identity.Application.DTOs.Responses;

public sealed record AuthorizationOverviewDto(
    bool ModuleGatingRunsFirst,
    bool FeatureGatingRunsBeforeAuthorization,
    bool DbPermissionGateImplemented,
    string PermissionSource,
    string CurrentDevelopmentUserLookup,
    IReadOnlyCollection<string> ResolutionOrder,
    IReadOnlyCollection<RoleOverviewDto> Roles,
    IReadOnlyCollection<PermissionOverviewDto> Permissions);
