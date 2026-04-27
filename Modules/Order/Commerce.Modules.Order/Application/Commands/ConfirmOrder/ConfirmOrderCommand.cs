using Commerce.Modules.Order.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Order.Application.Commands.ConfirmOrder;

public sealed record ConfirmOrderCommand : IRequest<ApiResponse<OrderDto>>
{
    public Guid Id { get; init; }

    public string? Note { get; init; }
}
