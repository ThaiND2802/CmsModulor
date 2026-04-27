namespace Commerce.Modules.Sale.Application.DTOs.Responses;

public sealed record SaleListItemDto(
    Guid Id,
    string SaleNumber,
    Guid? CustomerId,
    string CustomerEmail,
    string Status,
    decimal TotalAmount,
    string Currency,
    int ItemCount,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
