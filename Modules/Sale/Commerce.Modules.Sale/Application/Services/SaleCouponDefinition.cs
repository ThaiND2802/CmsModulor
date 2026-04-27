using Commerce.Modules.Sale.Domain;

namespace Commerce.Modules.Sale.Application.Services;

public sealed record SaleCouponDefinition(
    string Code,
    SaleCouponType Type,
    decimal Value,
    DateTime? ExpiresAtUtc = null);
