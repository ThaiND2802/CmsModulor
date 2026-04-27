using Commerce.Modules.Sale.Application.Commands.RemoveSaleItem;
using Commerce.Modules.Sale.Domain;
using Commerce.Modules.Sale.Tests.TestCommon;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Sale.Tests.Application.Commands.RemoveSaleItem;

public sealed class RemoveSaleItemHandlerTests
{
    [Fact]
    public async Task Handle_RemovesItemAndKeepsDraft()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        await using var seedContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var sale = SaleTestFixture.CreateSaleEntity();
        sale.Items.Add(new SaleItem
        {
            Id = Guid.NewGuid(),
            SaleId = sale.Id,
            ProductId = Guid.NewGuid(),
            ProductName = "Product",
            ProductSku = "SKU-001",
            VariantId = Guid.NewGuid(),
            VariantName = "Variant",
            UnitPrice = 20m,
            Quantity = 1,
            DiscountAmount = 1m,
            TotalAmount = 19m
        });
        seedContext.Sales.Add(sale);
        await seedContext.SaveChangesAsync();
        var itemId = sale.Items.Single().Id;

        await using var dbContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var handler = new RemoveSaleItemHandler(
            dbContext,
            SaleTestFixture.CreateSalePricingService(),
            SaleTestFixture.CreateMapper());

        var response = await handler.Handle(new RemoveSaleItemCommand
        {
            SaleId = sale.Id,
            ItemId = itemId
        }, CancellationToken.None);

        var updatedSale = await dbContext.Sales.Include(x => x.Items).SingleAsync(x => x.Id == sale.Id);
        response.Status.Should().Be(200);
        updatedSale.Status.Should().Be(SaleStatus.Draft);
        updatedSale.Items.Should().BeEmpty();
        updatedSale.SubtotalAmount.Should().Be(0m);
        updatedSale.TotalAmount.Should().Be(4m);
    }
}
