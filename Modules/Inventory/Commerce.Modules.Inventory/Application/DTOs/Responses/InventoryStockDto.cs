namespace Commerce.Modules.Inventory.Application.DTOs.Responses;

public sealed record InventoryStockDto(
    Guid Id,
    Guid VariantId,
    string Sku,
    int OnHandQuantity,
    int ReservedQuantity,
    int AvailableQuantity,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
