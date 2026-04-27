using MediatR;

namespace Commerce.Modules.Inventory.Application.Events;

public sealed record StockReserved(
    Guid ReservationId,
    Guid OrderId,
    IReadOnlyList<StockReservedItem> Items) : INotification;

public sealed record StockReservedItem(
    Guid InventoryItemId,
    Guid VariantId,
    string Sku,
    int Quantity);
