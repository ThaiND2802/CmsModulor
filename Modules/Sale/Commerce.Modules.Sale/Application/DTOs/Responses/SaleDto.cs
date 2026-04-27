namespace Commerce.Modules.Sale.Application.DTOs.Responses;

public sealed record SaleDto(
    Guid Id,
    string SaleNumber,
    Guid? CustomerId,
    string CustomerEmail,
    string? CustomerPhone,
    string Status,
    string? Notes,
    DateTime? ExpiresAtUtc,
    SaleCouponDto? Coupon,
    SaleAddressDto? ShippingAddress,
    SaleAddressDto? BillingAddress,
    SaleTotalsDto Totals,
    IReadOnlyList<SaleItemDto> Items,
    IReadOnlyList<SaleStatusHistoryDto> StatusHistory,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
