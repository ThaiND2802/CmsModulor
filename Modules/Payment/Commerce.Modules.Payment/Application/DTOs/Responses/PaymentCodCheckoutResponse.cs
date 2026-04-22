namespace Commerce.Modules.Payment.Application.DTOs.Responses;

public sealed record PaymentCodCheckoutResponse
{
    public string Module { get; init; } = default!;

    public string Feature { get; init; } = default!;

    public string Status { get; init; } = default!;

    public string PaymentMethod { get; init; } = default!;
}
