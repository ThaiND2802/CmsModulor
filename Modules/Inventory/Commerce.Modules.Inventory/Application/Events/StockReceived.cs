using MediatR;

namespace Commerce.Modules.Inventory.Application.Events;

public sealed record StockReceived(
    Guid InventoryItemId,
    Guid VariantId,
    string Sku,
    int QuantityReceived,
    int OnHandQuantity,
    int ReservedQuantity) : INotification;
