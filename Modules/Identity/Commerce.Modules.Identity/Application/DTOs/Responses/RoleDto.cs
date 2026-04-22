namespace Commerce.Modules.Identity.Application.DTOs.Responses;

public sealed record RoleDto
{
    public Guid Id { get; init; }

    public string Code { get; init; } = default!;

    public string Name { get; init; } = default!;

    public string? Description { get; init; }

    public bool IsActive { get; init; }

    public bool IsSystem { get; init; }

    public DateTime CreatedAtUtc { get; init; }

    public string? CreatedBy { get; init; }

    public DateTime? UpdatedAtUtc { get; init; }

    public string? UpdatedBy { get; init; }
}
