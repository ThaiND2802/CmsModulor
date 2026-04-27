namespace Commerce.Modules.Order.Application.DTOs.Responses;

public sealed record OrderListItemDto(
    Guid Id,
    string OrderNumber,
    Guid? CustomerId,
    string CustomerEmail,
    string Status,
    decimal TotalAmount,
    string Currency,
    int ItemCount,
    DateTime CreatedAtUtc);
