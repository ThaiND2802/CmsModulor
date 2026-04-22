namespace Commerce.Modules.Payment.Application.DTOs.Responses;

public sealed record DeletedPaymentMethodDto
{
    public Guid Id { get; init; }

    public string Code { get; init; } = default!;

    public string Name { get; init; } = default!;

    public DateTime? DeletedAtUtc { get; init; }

    public string? DeletedBy { get; init; }
}
