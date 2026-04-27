namespace Commerce.Modules.Inventory.Contracts.Responses;

public sealed record InventoryAvailabilityResponse(
    Guid Id,
    Guid OrderId,
    string Status,
    DateTime CreatedAtUtc,
    DateTime? ReleasedAtUtc,
    IReadOnlyList<InventoryAvailabilityItemResponse> Items);

public sealed record InventoryAvailabilityItemResponse(
    Guid InventoryItemId,
    Guid VariantId,
    string Sku,
    int Quantity);
