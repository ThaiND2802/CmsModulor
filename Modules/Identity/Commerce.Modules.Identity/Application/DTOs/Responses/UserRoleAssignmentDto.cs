namespace Commerce.Modules.Identity.Application.DTOs.Responses;

public sealed record UserRoleAssignmentDto
{
    public Guid RoleId { get; init; }

    public string RoleCode { get; init; } = default!;

    public string RoleName { get; init; } = default!;

    public bool IsActive { get; init; }

    public DateTime AssignedAtUtc { get; init; }

    public string? AssignedBy { get; init; }

    public DateTime? ExpiresAtUtc { get; init; }
}
