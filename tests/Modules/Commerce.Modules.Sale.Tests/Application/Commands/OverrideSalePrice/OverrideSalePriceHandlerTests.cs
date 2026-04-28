using Commerce.Modules.Sale.Application.Commands.OverrideSalePrice;
using Commerce.Modules.Sale.Application.Services;
using Commerce.Modules.Sale.Contracts;
using Commerce.Modules.Sale.Domain;
using Commerce.Modules.Sale.Tests.TestCommon;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Sale.Tests.Application.Commands.OverrideSalePrice;

public sealed class OverrideSalePriceHandlerTests
{
    [Fact]
    public async Task Handle_RequiresOverridePermission()
    {
        await using var dbContext = SaleTestFixture.CreateSaleDbContext();
        var sale = SaleTestFixture.CreatePricedSaleEntity();
        dbContext.Sales.Add(sale);
        await dbContext.SaveChangesAsync();

        var handler = new OverrideSalePriceHandler(
            dbContext,
            SaleTestFixture.CreateSalePricingService(),
            new SaleSubmissionService(dbContext, SaleTestFixture.CreateInventoryModule()),
            SaleTestFixture.CreatePermissionGate(false),
            SaleTestFixture.CreateFeatureGate(("Sale.OverridePrice", true)),
            SaleTestFixture.CreateCurrentUser("user-1", "tester"),
            SaleTestFixture.CreateMapper());

        var act = () => handler.Handle(new OverrideSalePriceCommand
        {
            SaleId = sale.Id,
            ItemId = sale.Items.Single().Id,
            OverridePrice = 8m,
            Reason = "Manager approval"
        }, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenAppException>()
            .WithMessage($"*{SalePermissions.SalesOverridePrice}*");
    }

    [Fact]
    public async Task Handle_OverridesPriceAndWritesAuditTrail()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        await using var seedContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var sale = SaleTestFixture.CreatePricedSaleEntity();
        seedContext.Sales.Add(sale);
        await seedContext.SaveChangesAsync();

        await using var dbContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var handler = new OverrideSalePriceHandler(
            dbContext,
            SaleTestFixture.CreateSalePricingService(),
            new SaleSubmissionService(dbContext, SaleTestFixture.CreateInventoryModule()),
            SaleTestFixture.CreatePermissionGate(true),
            SaleTestFixture.CreateFeatureGate(("Sale.OverridePrice", true)),
            SaleTestFixture.CreateCurrentUser("user-1", "tester"),
            SaleTestFixture.CreateMapper());

        var response = await handler.Handle(new OverrideSalePriceCommand
        {
            SaleId = sale.Id,
            ItemId = sale.Items.Single().Id,
            OverridePrice = 8m,
            Reason = "Manager approval"
        }, CancellationToken.None);

        var updatedSale = await dbContext.Sales
            .Include(x => x.Items)
            .Include(x => x.StatusHistory)
            .SingleAsync(x => x.Id == sale.Id);

        response.Status.Should().Be(200);
        updatedSale.Status.Should().Be(SaleStatus.Draft);
        updatedSale.TotalAmount.Should().Be(22m);
        updatedSale.Items.Single().OverridePrice.Should().Be(8m);
        updatedSale.Items.Single().OverrideReason.Should().Be("Manager approval");
        updatedSale.StatusHistory.Should().Contain(x => x.Note != null && x.Note.Contains("Price override applied"));
    }

    [Fact]
    public async Task Handle_WhenOverrideFeatureDisabled_RejectsDefaultFlow()
    {
        await using var dbContext = SaleTestFixture.CreateSaleDbContext();
        var sale = SaleTestFixture.CreatePricedSaleEntity();
        dbContext.Sales.Add(sale);
        await dbContext.SaveChangesAsync();

        var handler = new OverrideSalePriceHandler(
            dbContext,
            SaleTestFixture.CreateSalePricingService(),
            new SaleSubmissionService(dbContext, SaleTestFixture.CreateInventoryModule()),
            SaleTestFixture.CreatePermissionGate(true),
            SaleTestFixture.CreateFeatureGate(("Sale.OverridePrice", false)),
            SaleTestFixture.CreateCurrentUser("user-1", "tester"),
            SaleTestFixture.CreateMapper());

        var act = () => handler.Handle(new OverrideSalePriceCommand
        {
            SaleId = sale.Id,
            ItemId = sale.Items.Single().Id,
            OverridePrice = 8m,
            Reason = "Manager approval"
        }, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundAppException>()
            .WithMessage("*disabled in the MVP runtime*");
    }
}
