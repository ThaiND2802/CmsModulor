using Commerce.Modules.Payment.Application.DTOs.Responses;

namespace Commerce.Modules.Payment.Application.Commands.CreateCodCheckout;

public sealed class CreateCodCheckoutHandler
{
    public Task<PaymentCodCheckoutResponse> HandleAsync(CreateCodCheckoutCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        return Task.FromResult(new PaymentCodCheckoutResponse("Payment", "Payment.COD", "CheckoutCreated", "CashOnDelivery"));
    }
}
