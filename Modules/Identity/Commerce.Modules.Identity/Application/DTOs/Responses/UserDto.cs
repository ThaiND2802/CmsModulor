namespace Commerce.Modules.Identity.Application.DTOs.Responses;

public sealed record UserDto
{
    public Guid Id { get; init; }

    public string UserName { get; init; } = default!;

    public string NormalizedUserName { get; init; } = default!;

    public string? DisplayName { get; init; }

    public string? Email { get; init; }

    public bool IsActive { get; init; }

    public bool IsSystem { get; init; }

    public DateTime? LastLoginAtUtc { get; init; }

    public DateTime CreatedAtUtc { get; init; }

    public string? CreatedBy { get; init; }

    public DateTime? UpdatedAtUtc { get; init; }

    public string? UpdatedBy { get; init; }
}
