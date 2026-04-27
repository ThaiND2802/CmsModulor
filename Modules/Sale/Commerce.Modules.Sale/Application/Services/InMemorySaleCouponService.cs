using Commerce.Modules.Sale.Domain;

namespace Commerce.Modules.Sale.Application.Services;

public sealed class InMemorySaleCouponService : ISaleCouponService
{
    private static readonly IReadOnlyDictionary<string, SaleCouponDefinition> Coupons =
        new Dictionary<string, SaleCouponDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["SAVE10"] = new("SAVE10", SaleCouponType.Percentage, 10m),
            ["LESS5"] = new("LESS5", SaleCouponType.FixedAmount, 5m),
            ["FREESHIP"] = new("FREESHIP", SaleCouponType.FreeShipping, 0m)
        };

    public Task<SaleCouponDefinition?> GetByCodeAsync(string code, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        Coupons.TryGetValue(code.Trim(), out var coupon);
        return Task.FromResult(coupon);
    }
}
