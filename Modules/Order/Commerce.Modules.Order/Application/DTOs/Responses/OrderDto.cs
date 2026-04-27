namespace Commerce.Modules.Order.Application.DTOs.Responses;

public sealed record OrderDto(
    Guid Id,
    string OrderNumber,
    Guid? CustomerId,
    string CustomerEmail,
    string? CustomerPhone,
    string Status,
    string? Notes,
    OrderAddressDto ShippingAddress,
    OrderAddressDto? BillingAddress,
    decimal SubtotalAmount,
    decimal DiscountAmount,
    decimal ShippingAmount,
    decimal TaxAmount,
    decimal TotalAmount,
    string Currency,
    string? CancelReason,
    IReadOnlyList<OrderItemDto> Items,
    IReadOnlyList<OrderStatusHistoryDto> StatusHistory,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
