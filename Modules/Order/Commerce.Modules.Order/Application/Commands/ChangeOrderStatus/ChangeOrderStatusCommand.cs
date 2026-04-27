using Commerce.Modules.Order.Application.DTOs.Responses;
using Commerce.Modules.Order.Domain;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Order.Application.Commands.ChangeOrderStatus;

public sealed record ChangeOrderStatusCommand : IRequest<ApiResponse<OrderDto>>
{
    public Guid Id { get; init; }

    public OrderStatus ToStatus { get; init; }

    public string? Note { get; init; }
}
