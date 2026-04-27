using Commerce.Modules.Order.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Order.Application.Queries.GetOrderByNumber;

public sealed record GetOrderByNumberQuery : IRequest<ApiResponse<OrderDto>>
{
    public string OrderNumber { get; init; } = default!;
}
