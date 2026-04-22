using Commerce.Modules.Payment.Application.DTOs.Requests;

namespace Commerce.Modules.Payment.Application.Commands.UpdatePaymentMethod;

public sealed record UpdatePaymentMethodCommand(Guid Id, UpdatePaymentMethodRequest Request);
