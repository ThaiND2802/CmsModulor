using MediatR;

namespace Commerce.Modules.Sale.Application.Commands.UpdateSaleCustomer;

public sealed record UpdateSaleCustomerCommand : IRequest
{
    public Guid Id { get; init; }

    public Guid? CustomerId { get; init; }

    public string CustomerEmail { get; init; } = default!;

    public string? CustomerPhone { get; init; }
}
