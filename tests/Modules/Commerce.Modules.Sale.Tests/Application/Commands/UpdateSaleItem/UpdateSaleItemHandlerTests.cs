using Commerce.Modules.Sale.Application.Commands.UpdateSaleItem;
using Commerce.Modules.Sale.Domain;
using Commerce.Modules.Sale.Tests.TestCommon;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Sale.Tests.Application.Commands.UpdateSaleItem;

public sealed class UpdateSaleItemHandlerTests
{
    [Fact]
    public async Task Handle_UpdatesSnapshotAndTotals()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        await using var seedContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var sale = SaleTestFixture.CreateSaleEntity();
        sale.Items.Add(new SaleItem
        {
            Id = Guid.NewGuid(),
            SaleId = sale.Id,
            ProductId = Guid.NewGuid(),
            ProductName = "Old Product",
            ProductSku = "OLD-SKU",
            VariantId = Guid.NewGuid(),
            VariantName = "Old Variant",
            UnitPrice = 10m,
            Quantity = 1,
            DiscountAmount = 0m,
            TotalAmount = 10m
        });
        seedContext.Sales.Add(sale);
        await seedContext.SaveChangesAsync();

        var variant = SaleTestFixture.CreateCatalogVariantSummary(sku: "NEW-SKU", productName: "New Product", variantName: "Large", unitPrice: 15m);
        var existingItemId = sale.Items.Single().Id;
        await using var dbContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var handler = new UpdateSaleItemHandler(
            dbContext,
            SaleTestFixture.CreateCatalogVariantLookup(variant),
            SaleTestFixture.CreateSalePricingService(),
            SaleTestFixture.CreateMapper());

        var response = await handler.Handle(new UpdateSaleItemCommand
        {
            SaleId = sale.Id,
            ItemId = existingItemId,
            VariantId = variant.Id,
            Quantity = 3,
            DiscountAmount = 2m
        }, CancellationToken.None);

        var updatedSale = await dbContext.Sales.Include(x => x.Items).SingleAsync(x => x.Id == sale.Id);
        var updatedItem = updatedSale.Items.Single();
        response.Status.Should().Be(200);
        updatedItem.ProductSku.Should().Be("NEW-SKU");
        updatedItem.ProductName.Should().Be("New Product");
        updatedItem.Quantity.Should().Be(3);
        updatedItem.TotalAmount.Should().Be(43m);
        updatedSale.SubtotalAmount.Should().Be(45m);
        updatedSale.TotalAmount.Should().Be(47m);
    }
}
