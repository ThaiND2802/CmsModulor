namespace Commerce.Modules.Payment.Application.DTOs.Responses;

public sealed record DeletePaymentMethodResponse(
    Guid Id,
    bool Deleted);
