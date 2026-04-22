namespace Commerce.Modules.Payment.Application.DTOs.Responses;

public sealed record PaymentMethodDto
{
    public Guid Id { get; init; }

    public string Code { get; init; } = default!;

    public string Name { get; init; } = default!;

    public bool IsActive { get; init; }

    public DateTime CreatedAtUtc { get; init; }

    public string? CreatedBy { get; init; }

    public DateTime? UpdatedAtUtc { get; init; }

    public string? UpdatedBy { get; init; }
}
