using Commerce.Modules.Order.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Order.Application.Commands.UpdateOrderNotes;

public sealed record UpdateOrderNotesCommand : IRequest<ApiResponse<OrderDto>>
{
    public Guid Id { get; init; }

    public string? Notes { get; init; }
}
