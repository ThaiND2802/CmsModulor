using Commerce.Modules.Payment.Application.DTOs.Requests;

namespace Commerce.Modules.Payment.Application.Commands.CreatePaymentMethod;

public sealed record CreatePaymentMethodCommand(CreatePaymentMethodRequest Request);
