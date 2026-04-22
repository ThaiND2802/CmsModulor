using Commerce.Modules.Payment.Application.DTOs.Responses;

namespace Commerce.Modules.Payment.Application.Queries.GetCodHealth;

public sealed class GetCodHealthHandler
{
    public Task<PaymentCodHealthResponse> HandleAsync(GetCodHealthQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        return Task.FromResult(new PaymentCodHealthResponse("Payment", "Payment.COD", "Healthy"));
    }
}
