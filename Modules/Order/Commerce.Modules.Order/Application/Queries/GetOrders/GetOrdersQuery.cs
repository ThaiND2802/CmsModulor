using Commerce.Modules.Order.Application.DTOs.Responses;
using Commerce.Modules.Order.Domain;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Order.Application.Queries.GetOrders;

public sealed record GetOrdersQuery : IRequest<PagedApiResponse<OrderListItemDto>>
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;

    public string? Search { get; init; }

    public Guid? CustomerId { get; init; }

    public OrderStatus? Status { get; init; }

    public DateTime? FromDate { get; init; }

    public DateTime? ToDate { get; init; }

    public string? SortBy { get; init; }

    public bool SortDescending { get; init; } = true;
}
