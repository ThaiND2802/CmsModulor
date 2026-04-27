using MediatR;

namespace Commerce.Modules.Sale.Application.Commands.MarkSalePaid;

public sealed record MarkSalePaidCommand(
    Guid SaleId,
    decimal Amount,
    string? PaymentReference = null
) : IRequest;
