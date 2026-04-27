using Commerce.Modules.Inventory.Application.Commands.ReleaseStock;
using Commerce.Modules.Inventory.Application.Commands.ReserveStock;
using Commerce.Modules.Inventory.Contracts;
using Commerce.Modules.Inventory.Contracts.Requests;
using Commerce.Modules.Inventory.Contracts.Responses;
using MediatR;

namespace Commerce.Modules.Inventory.Application.Services;

public sealed class InventoryModule : IInventoryModule
{
    private readonly IMediator _mediator;

    public InventoryModule(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task<InventoryAvailabilityResponse?> ReserveStockAsync(ReserveStockRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await _mediator.Send(
            new ReserveStockCommand
            {
                OrderId = request.OrderId,
                Items = request.Items
                    .Select(static item => new Commerce.Modules.Inventory.Application.Commands.ReserveStock.ReserveStockItemRequest(item.VariantId, item.Quantity))
                    .ToArray()
            },
            cancellationToken);

        return response.Data is null ? null : MapResponse(response.Data);
    }

    public async Task<InventoryAvailabilityResponse?> ReleaseStockAsync(ReleaseStockRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var response = await _mediator.Send(new ReleaseStockCommand(request.OrderId), cancellationToken);
        return response.Data is null ? null : MapResponse(response.Data);
    }

    private static InventoryAvailabilityResponse MapResponse(Application.DTOs.Responses.StockReservationDto response)
    {
        return new InventoryAvailabilityResponse(
            response.Id,
            response.OrderId,
            response.Status,
            response.CreatedAtUtc,
            response.ReleasedAtUtc,
            response.Items
                .Select(static item => new InventoryAvailabilityItemResponse(item.InventoryItemId, item.VariantId, item.Sku, item.Quantity))
                .ToArray());
    }
}
