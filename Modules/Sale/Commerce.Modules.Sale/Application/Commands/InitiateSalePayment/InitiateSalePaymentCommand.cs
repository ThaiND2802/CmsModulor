using MediatR;

namespace Commerce.Modules.Sale.Application.Commands.InitiateSalePayment;

public sealed record InitiateSalePaymentCommand(
    Guid SaleId,
    string? PaymentReference = null
) : IRequest;
