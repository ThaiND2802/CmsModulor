namespace Commerce.Modules.Payment.Application.DTOs.Responses;

public sealed record GetPaymentMethodsResult
{
    public IReadOnlyCollection<PaymentMethodDto> Items { get; init; } = [];

    public int Total { get; init; }

    public int Page { get; init; }

    public int PageSize { get; init; }
}
