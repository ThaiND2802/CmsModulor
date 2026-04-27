using Commerce.Modules.Inventory.Application.DTOs.Responses;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Inventory.Application.Commands.ReserveStock;

public sealed record ReserveStockCommand : IRequest<ApiResponse<StockReservationDto>>
{
    public Guid OrderId { get; init; }

    public IReadOnlyList<ReserveStockItemRequest> Items { get; init; } = [];
}

public sealed record ReserveStockItemRequest(
    Guid VariantId,
    int Quantity);
