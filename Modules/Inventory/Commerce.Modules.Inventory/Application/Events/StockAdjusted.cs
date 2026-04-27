using MediatR;

namespace Commerce.Modules.Inventory.Application.Events;

public sealed record StockAdjusted(
    Guid InventoryItemId,
    Guid VariantId,
    string Sku,
    int QuantityDelta,
    int OnHandQuantity,
    int ReservedQuantity) : INotification;
