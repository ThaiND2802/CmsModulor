using Commerce.Modules.Sale.Domain;
using MediatR;

namespace Commerce.Modules.Sale.Application.Commands.SelectPaymentMethod;

public sealed record SelectPaymentMethodCommand(
    Guid SaleId,
    PaymentMethod PaymentMethod
) : IRequest;
