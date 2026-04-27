namespace Commerce.Modules.Sale.Application.DTOs.Responses;

public sealed record SaleTotalsDto(
    decimal SubtotalAmount,
    decimal DiscountAmount,
    decimal ShippingAmount,
    decimal TaxAmount,
    decimal TotalAmount,
    string Currency);
