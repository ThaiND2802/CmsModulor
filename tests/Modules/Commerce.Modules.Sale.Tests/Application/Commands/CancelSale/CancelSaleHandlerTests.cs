using Commerce.Modules.Sale.Application.Commands.CancelSale;
using Commerce.Modules.Sale.Application.Services;
using Commerce.Modules.Sale.Domain;
using Commerce.Modules.Sale.Tests.TestCommon;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Sale.Tests.Application.Commands.CancelSale;

public sealed class CancelSaleHandlerTests
{
    [Fact]
    public async Task Handle_CancelsPricedSale()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        await using var seedContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var sale = SaleTestFixture.CreatePricedSaleEntity();
        seedContext.Sales.Add(sale);
        await seedContext.SaveChangesAsync();

        await using var dbContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var handler = new CancelSaleHandler(
            new SaleSubmissionService(dbContext, SaleTestFixture.CreateInventoryModule()),
            SaleTestFixture.CreateMapper());

        var response = await handler.Handle(
            new CancelSaleCommand { SaleId = sale.Id, Reason = "Operator cancelled sale." },
            CancellationToken.None);

        var updatedSale = await dbContext.Sales.Include(x => x.StatusHistory).SingleAsync(x => x.Id == sale.Id);
        response.Status.Should().Be(200);
        updatedSale.Status.Should().Be(SaleStatus.Cancelled);
        updatedSale.StatusHistory.Should().Contain(x => x.ToStatus == SaleStatus.Cancelled);
    }

    [Fact]
    public async Task Handle_RejectsSubmittedSale()
    {
        await using var dbContext = SaleTestFixture.CreateSaleDbContext();
        var sale = SaleTestFixture.CreateSaleEntity(SaleStatus.Submitted);
        dbContext.Sales.Add(sale);
        await dbContext.SaveChangesAsync();

        var handler = new CancelSaleHandler(
            new SaleSubmissionService(dbContext, SaleTestFixture.CreateInventoryModule()),
            SaleTestFixture.CreateMapper());

        var act = () => handler.Handle(new CancelSaleCommand { SaleId = sale.Id }, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleAppException>();
    }
}
