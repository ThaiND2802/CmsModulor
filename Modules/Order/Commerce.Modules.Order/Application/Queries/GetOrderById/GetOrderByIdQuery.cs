using Commerce.Modules.Order.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Order.Application.Queries.GetOrderById;

public sealed record GetOrderByIdQuery : IRequest<ApiResponse<OrderDto>>
{
    public Guid Id { get; init; }
}
