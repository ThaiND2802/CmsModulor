namespace Commerce.Modules.Inventory.Contracts.Requests;

public sealed record ReserveStockRequest(
    Guid OrderId,
    IReadOnlyList<ReserveStockItemRequest> Items);

public sealed record ReserveStockItemRequest(
    Guid VariantId,
    int Quantity);
