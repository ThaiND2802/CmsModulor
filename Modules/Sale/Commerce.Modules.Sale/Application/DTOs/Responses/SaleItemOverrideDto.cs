namespace Commerce.Modules.Sale.Application.DTOs.Responses;

public sealed record SaleItemOverrideDto(
    decimal OverridePrice,
    string Reason,
    string? OverriddenBy,
    DateTime OverriddenAtUtc);
