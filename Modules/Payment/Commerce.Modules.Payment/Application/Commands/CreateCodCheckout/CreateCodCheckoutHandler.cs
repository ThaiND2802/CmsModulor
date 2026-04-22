using Commerce.Modules.Payment.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Payment.Application.Commands.CreateCodCheckout;

public sealed class CreateCodCheckoutHandler : IRequestHandler<CreateCodCheckoutCommand, ApiResponse<PaymentCodCheckoutResponse>>
{
    public Task<ApiResponse<PaymentCodCheckoutResponse>> Handle(CreateCodCheckoutCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        return Task.FromResult(new ApiResponse<PaymentCodCheckoutResponse>
        {
            Status = 200,
            Data = new PaymentCodCheckoutResponse
            {
                Module = "Payment",
                Feature = "Payment.COD",
                Status = "CheckoutCreated",
                PaymentMethod = "CashOnDelivery"
            }
        });
    }
}
