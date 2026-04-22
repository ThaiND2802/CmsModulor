namespace Commerce.Modules.Payment.Application.DTOs.Responses;

public sealed record PaymentCodPermissionFlags
{
    public bool PaymentCodRead { get; init; }

    public bool PaymentCodCheckout { get; init; }
}
