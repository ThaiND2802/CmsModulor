namespace Commerce.Modules.Identity.Application.DTOs.Responses;

public sealed record UserPermissionAssignmentsResponse
{
    public Guid UserId { get; init; }

    public IReadOnlyList<UserPermissionAssignmentDto> Permissions { get; init; } = [];
}
