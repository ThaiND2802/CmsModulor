namespace Commerce.Modules.Identity.Application.DTOs.Responses;

public sealed record RolePermissionAssignmentsResponse
{
    public Guid RoleId { get; init; }

    public IReadOnlyList<RolePermissionAssignmentDto> Permissions { get; init; } = [];
}
