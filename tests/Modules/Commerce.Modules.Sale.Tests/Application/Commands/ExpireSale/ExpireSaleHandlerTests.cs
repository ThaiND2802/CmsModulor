using Commerce.Modules.Inventory.Contracts.Responses;
using Commerce.Modules.Sale.Application.Commands.ExpireSale;
using Commerce.Modules.Sale.Application.Commands.SubmitSale;
using Commerce.Modules.Sale.Application.Services;
using Commerce.Modules.Sale.Domain;
using Commerce.Modules.Sale.Tests.TestCommon;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Sale.Tests.Application.Commands.ExpireSale;

public sealed class ExpireSaleHandlerTests
{
    [Fact]
    public async Task Handle_ExpiresEligibleSale()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        await using var seedContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var sale = SaleTestFixture.CreatePricedSaleEntity();
        seedContext.Sales.Add(sale);
        await seedContext.SaveChangesAsync();

        await using var dbContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var handler = new ExpireSaleHandler(
            new SaleSubmissionService(dbContext, SaleTestFixture.CreateInventoryModule()),
            SaleTestFixture.CreateMapper());

        var response = await handler.Handle(new ExpireSaleCommand
        {
            SaleId = sale.Id,
            Reason = "Reservation window elapsed"
        }, CancellationToken.None);

        var updatedSale = await dbContext.Sales.Include(x => x.StatusHistory).SingleAsync(x => x.Id == sale.Id);
        response.Status.Should().Be(200);
        updatedSale.Status.Should().Be(SaleStatus.Expired);
        updatedSale.StatusHistory.Should().Contain(x => x.ToStatus == SaleStatus.Expired);
    }

    [Fact]
    public async Task Handle_PreventsSubmitAfterExpiry()
    {
        await using var dbContext = SaleTestFixture.CreateSaleDbContext();
        var sale = SaleTestFixture.CreatePricedSaleEntity();
        sale.Status = SaleStatus.Expired;
        dbContext.Sales.Add(sale);
        await dbContext.SaveChangesAsync();

        var submitHandler = new SubmitSaleHandler(
            new SaleSubmissionService(
                dbContext,
                SaleTestFixture.CreateInventoryModule(
                    SaleTestFixture.CreateInventoryAvailabilityResponse(
                        Guid.NewGuid(),
                        sale.Id,
                        new InventoryAvailabilityItemResponse(Guid.NewGuid(), sale.Items.First().VariantId!.Value, "SKU-001", 2)))),
            SaleTestFixture.CreateOrderModule(),
            SaleTestFixture.CreateMapper());

        var act = () => submitHandler.Handle(new SubmitSaleCommand { SaleId = sale.Id }, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleAppException>()
            .WithMessage("*Expired sales cannot be submitted*");
    }
}
