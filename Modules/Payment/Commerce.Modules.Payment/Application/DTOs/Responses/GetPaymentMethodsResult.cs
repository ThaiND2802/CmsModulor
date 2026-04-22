namespace Commerce.Modules.Payment.Application.DTOs.Responses;

public sealed record GetPaymentMethodsResult(
    IReadOnlyCollection<PaymentMethodDto> Items,
    int Total,
    int Page,
    int PageSize);
