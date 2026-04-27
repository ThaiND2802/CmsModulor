using MediatR;

namespace Commerce.Modules.Inventory.Application.Events;

public sealed record StockReleased(
    Guid ReservationId,
    Guid OrderId,
    IReadOnlyList<StockReleasedItem> Items) : INotification;

public sealed record StockReleasedItem(
    Guid InventoryItemId,
    Guid VariantId,
    string Sku,
    int Quantity);
