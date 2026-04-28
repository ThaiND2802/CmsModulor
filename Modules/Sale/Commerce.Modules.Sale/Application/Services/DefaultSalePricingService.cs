using Commerce.Modules.Sale.Domain;
using SaleEntity = Commerce.Modules.Sale.Domain.Sale;

namespace Commerce.Modules.Sale.Application.Services;

public sealed class DefaultSalePricingService : ISalePricingService
{
    public void Apply(SaleEntity sale)
    {
        ArgumentNullException.ThrowIfNull(sale);

        if (string.IsNullOrWhiteSpace(sale.CouponCode)
            && sale.CouponDiscountAmount == 0m
            && sale.BaseDiscountAmount == 0m
            && sale.DiscountAmount > 0m)
        {
            sale.BaseDiscountAmount = sale.DiscountAmount;
        }

        sale.SubtotalAmount = sale.Items.Sum(static x => (x.OverridePrice ?? x.UnitPrice) * x.Quantity);
        sale.CouponDiscountAmount = CalculateCouponDiscount(sale);
        sale.CouponShippingDiscountAmount = 0m;
        sale.DiscountAmount = sale.BaseDiscountAmount + sale.CouponDiscountAmount;
        sale.ShippingAmount = sale.BaseShippingAmount;
        sale.TotalAmount = sale.SubtotalAmount
            - sale.DiscountAmount
            - sale.Items.Sum(static x => x.DiscountAmount)
            + sale.ShippingAmount
            + sale.TaxAmount;
    }

    private static decimal CalculateCouponDiscount(SaleEntity sale)
    {
        if (string.IsNullOrWhiteSpace(sale.CouponCode) || string.IsNullOrWhiteSpace(sale.CouponType))
        {
            return 0m;
        }

        if (!Enum.TryParse<SaleCouponType>(sale.CouponType, ignoreCase: true, out var couponType))
        {
            return 0m;
        }

        var couponValue = sale.CouponValue.GetValueOrDefault();
        return couponType switch
        {
            SaleCouponType.FixedAmount => Math.Min(couponValue, sale.SubtotalAmount),
            SaleCouponType.Percentage => Math.Min(decimal.Round(sale.SubtotalAmount * (couponValue / 100m), 4), sale.SubtotalAmount),
            _ => 0m
        };
    }

}
