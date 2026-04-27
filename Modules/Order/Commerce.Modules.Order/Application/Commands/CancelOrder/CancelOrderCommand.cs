using Commerce.Modules.Order.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Order.Application.Commands.CancelOrder;

public sealed record CancelOrderCommand : IRequest<ApiResponse<OrderDto>>
{
    public Guid Id { get; init; }

    public string CancelReason { get; init; } = default!;
}
