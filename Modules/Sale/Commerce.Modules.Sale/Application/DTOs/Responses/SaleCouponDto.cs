namespace Commerce.Modules.Sale.Application.DTOs.Responses;

public sealed record SaleCouponDto(
    string Code,
    string Type,
    decimal Value);
