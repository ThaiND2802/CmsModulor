namespace Commerce.Modules.Inventory.Application.DTOs.Responses;

public sealed record StockMovementDto(
    Guid Id,
    Guid InventoryItemId,
    Guid VariantId,
    string Sku,
    string MovementType,
    int QuantityDelta,
    int OnHandAfter,
    int ReservedAfter,
    string Reason,
    string? ReferenceType,
    Guid? ReferenceId,
    DateTime CreatedAtUtc,
    Guid? CreatedByUserId);
