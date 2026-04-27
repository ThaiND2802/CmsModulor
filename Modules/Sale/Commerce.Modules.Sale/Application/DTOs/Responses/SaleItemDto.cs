namespace Commerce.Modules.Sale.Application.DTOs.Responses;

public sealed record SaleItemDto(
    Guid Id,
    Guid? ProductId,
    string ProductName,
    string ProductSku,
    Guid? VariantId,
    string? VariantName,
    decimal UnitPrice,
    int Quantity,
    decimal DiscountAmount,
    decimal TotalAmount,
    SaleItemOverrideDto? Override);
