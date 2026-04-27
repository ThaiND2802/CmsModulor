namespace Commerce.Modules.Order.Application.DTOs.Responses;

public sealed record OrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductSku,
    Guid? VariantId,
    string? VariantName,
    decimal UnitPrice,
    int Quantity,
    decimal DiscountAmount,
    decimal TotalAmount);
