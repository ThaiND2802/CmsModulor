using Commerce.Modules.Order.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Order.Application.Queries.GetOrdersByCustomer;

public sealed record GetOrdersByCustomerQuery : IRequest<PagedApiResponse<OrderListItemDto>>
{
    public Guid CustomerId { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}
