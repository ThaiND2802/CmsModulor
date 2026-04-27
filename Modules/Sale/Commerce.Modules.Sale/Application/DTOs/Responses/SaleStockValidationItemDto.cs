namespace Commerce.Modules.Sale.Application.DTOs.Responses;

public sealed record SaleStockValidationItemDto(
    Guid VariantId,
    int RequestedQuantity,
    bool Available,
    string? Message);
