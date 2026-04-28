using Commerce.Modules.Sale.Application.Commands.ApplyCoupon;
using Commerce.Modules.Sale.Application.Services;
using Commerce.Modules.Sale.Domain;
using Commerce.Modules.Sale.Tests.TestCommon;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Sale.Tests.Application.Commands.ApplyCoupon;

public sealed class ApplyCouponHandlerTests
{
    [Fact]
    public async Task Handle_AppliesValidCouponAndMovesSaleToDraft()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        await using var seedContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var sale = SaleTestFixture.CreatePricedSaleEntity();
        seedContext.Sales.Add(sale);
        await seedContext.SaveChangesAsync();

        await using var dbContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var handler = new ApplyCouponHandler(
            dbContext,
            SaleTestFixture.CreateSaleCouponService(new SaleCouponDefinition("SAVE10", SaleCouponType.Percentage, 10m)),
            SaleTestFixture.CreateSalePricingService(),
            SaleTestFixture.CreateMapper());

        var response = await handler.Handle(new ApplyCouponCommand
        {
            SaleId = sale.Id,
            Code = "SAVE10"
        }, CancellationToken.None);

        var updatedSale = await dbContext.Sales.SingleAsync(x => x.Id == sale.Id);
        response.Status.Should().Be(200);
        updatedSale.Status.Should().Be(SaleStatus.Draft);
        updatedSale.CouponCode.Should().Be("SAVE10");
        updatedSale.CouponDiscountAmount.Should().Be(2m);
        updatedSale.TotalAmount.Should().Be(19m);
    }

    [Fact]
    public async Task Handle_RejectsInvalidCoupon()
    {
        await using var dbContext = SaleTestFixture.CreateSaleDbContext();
        var sale = SaleTestFixture.CreatePricedSaleEntity();
        dbContext.Sales.Add(sale);
        await dbContext.SaveChangesAsync();

        var handler = new ApplyCouponHandler(
            dbContext,
            SaleTestFixture.CreateSaleCouponService(),
            SaleTestFixture.CreateSalePricingService(),
            SaleTestFixture.CreateMapper());

        var act = () => handler.Handle(new ApplyCouponCommand
        {
            SaleId = sale.Id,
            Code = "BADCODE"
        }, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleAppException>()
            .WithMessage("*invalid*");
    }

    [Fact]
    public async Task Handle_RejectsExpiredCoupon()
    {
        await using var dbContext = SaleTestFixture.CreateSaleDbContext();
        var sale = SaleTestFixture.CreatePricedSaleEntity();
        dbContext.Sales.Add(sale);
        await dbContext.SaveChangesAsync();

        var handler = new ApplyCouponHandler(
            dbContext,
            SaleTestFixture.CreateSaleCouponService(new SaleCouponDefinition("OLD10", SaleCouponType.Percentage, 10m, DateTime.UtcNow.AddMinutes(-1))),
            SaleTestFixture.CreateSalePricingService(),
            SaleTestFixture.CreateMapper());

        var act = () => handler.Handle(new ApplyCouponCommand
        {
            SaleId = sale.Id,
            Code = "OLD10"
        }, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleAppException>()
            .WithMessage("*expired*");
    }

    [Fact]
    public async Task Handle_RejectsUnsupportedCouponType()
    {
        await using var dbContext = SaleTestFixture.CreateSaleDbContext();
        var sale = SaleTestFixture.CreatePricedSaleEntity();
        dbContext.Sales.Add(sale);
        await dbContext.SaveChangesAsync();

        var handler = new ApplyCouponHandler(
            dbContext,
            SaleTestFixture.CreateSaleCouponService(new SaleCouponDefinition("FREESHIP", (SaleCouponType)999, 0m)),
            SaleTestFixture.CreateSalePricingService(),
            SaleTestFixture.CreateMapper());

        var act = () => handler.Handle(new ApplyCouponCommand
        {
            SaleId = sale.Id,
            Code = "FREESHIP"
        }, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleAppException>()
            .WithMessage("*not supported in the MVP runtime*");
    }
}
