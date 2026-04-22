namespace Commerce.Modules.Payment.Application.DTOs.Responses;

public sealed record PaymentCodCheckoutResponse(
    string Module,
    string Feature,
    string Status,
    string PaymentMethod);
