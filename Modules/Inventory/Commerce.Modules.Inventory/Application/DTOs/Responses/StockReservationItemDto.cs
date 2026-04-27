namespace Commerce.Modules.Inventory.Application.DTOs.Responses;

public sealed record StockReservationItemDto(
    Guid InventoryItemId,
    Guid VariantId,
    string Sku,
    int Quantity);
