namespace Commerce.Modules.Identity.Application.DTOs.Responses;

public sealed record RolePermissionAssignmentDto
{
    public Guid PermissionId { get; init; }

    public string PermissionCode { get; init; } = default!;

    public string PermissionName { get; init; } = default!;

    public string Module { get; init; } = default!;

    public string? Feature { get; init; }

    public string GroupName { get; init; } = default!;

    public string Effect { get; init; } = default!;

    public DateTime AssignedAtUtc { get; init; }

    public string? AssignedBy { get; init; }
}
