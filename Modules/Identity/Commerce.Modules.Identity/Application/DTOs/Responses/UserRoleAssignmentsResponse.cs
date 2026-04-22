namespace Commerce.Modules.Identity.Application.DTOs.Responses;

public sealed record UserRoleAssignmentsResponse
{
    public Guid UserId { get; init; }

    public IReadOnlyList<UserRoleAssignmentDto> Roles { get; init; } = [];
}
