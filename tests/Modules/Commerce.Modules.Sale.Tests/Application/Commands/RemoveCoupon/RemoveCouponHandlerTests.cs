using Commerce.Modules.Sale.Application.Commands.RemoveCoupon;
using Commerce.Modules.Sale.Domain;
using Commerce.Modules.Sale.Tests.TestCommon;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Sale.Tests.Application.Commands.RemoveCoupon;

public sealed class RemoveCouponHandlerTests
{
    [Fact]
    public async Task Handle_RemovesCouponAndRecalculatesTotals()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        await using var seedContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var sale = SaleTestFixture.CreatePricedSaleEntity();
        sale.CouponCode = "SAVE10";
        sale.CouponType = SaleCouponType.Percentage.ToString();
        sale.CouponValue = 10m;
        sale.BaseShippingAmount = sale.ShippingAmount;
        sale.BaseDiscountAmount = 0m;
        sale.CouponDiscountAmount = 2m;
        sale.DiscountAmount = 2m;
        sale.TotalAmount = 24m;
        seedContext.Sales.Add(sale);
        await seedContext.SaveChangesAsync();

        await using var dbContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var handler = new RemoveCouponHandler(
            dbContext,
            SaleTestFixture.CreateSalePricingService(),
            SaleTestFixture.CreateMapper());

        var response = await handler.Handle(new RemoveCouponCommand
        {
            SaleId = sale.Id
        }, CancellationToken.None);

        var updatedSale = await dbContext.Sales.SingleAsync(x => x.Id == sale.Id);
        response.Status.Should().Be(200);
        updatedSale.CouponCode.Should().BeNull();
        updatedSale.CouponDiscountAmount.Should().Be(0m);
        updatedSale.TotalAmount.Should().Be(26m);
    }
}
