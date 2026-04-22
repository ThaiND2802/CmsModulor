using Commerce.Modules.Payment.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Payment.Application.Queries.GetCodHealth;

public sealed class GetCodHealthHandler : IRequestHandler<GetCodHealthQuery, ApiResponse<PaymentCodHealthResponse>>
{
    public Task<ApiResponse<PaymentCodHealthResponse>> Handle(GetCodHealthQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        return Task.FromResult(new ApiResponse<PaymentCodHealthResponse>
        {
            Status = 200,
            Data = new PaymentCodHealthResponse
            {
                Module = "Payment",
                Feature = "Payment.COD",
                Status = "Healthy"
            }
        });
    }
}
