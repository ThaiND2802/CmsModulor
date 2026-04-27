using Commerce.Modules.Order.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Order.Application.Queries.GetMyOrders;

public sealed record GetMyOrdersQuery : IRequest<PagedApiResponse<OrderListItemDto>>
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}
