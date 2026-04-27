using Commerce.Modules.Sale.Application.Commands.RepriceSale;
using Commerce.Modules.Sale.Domain;
using Commerce.Modules.Sale.Tests.TestCommon;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Sale.Tests.Application.Commands.RepriceSale;

public sealed class RepriceSaleHandlerTests
{
    [Fact]
    public async Task Handle_RecalculatesSaleTotals()
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
            UnitPrice = 10m,
            Quantity = 3,
            DiscountAmount = 2m,
            TotalAmount = 28m
        });
        seedContext.Sales.Add(sale);
        await seedContext.SaveChangesAsync();

        await using var dbContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var handler = new RepriceSaleHandler(
            dbContext,
            SaleTestFixture.CreateSalePricingService(),
            SaleTestFixture.CreateMapper());

        var response = await handler.Handle(new RepriceSaleCommand
        {
            SaleId = sale.Id,
            DiscountAmount = 4m,
            ShippingAmount = 7m,
            TaxAmount = 3m
        }, CancellationToken.None);

        var updatedSale = await dbContext.Sales.SingleAsync(x => x.Id == sale.Id);
        response.Status.Should().Be(200);
        updatedSale.SubtotalAmount.Should().Be(30m);
        updatedSale.DiscountAmount.Should().Be(4m);
        updatedSale.ShippingAmount.Should().Be(7m);
        updatedSale.TaxAmount.Should().Be(3m);
        updatedSale.TotalAmount.Should().Be(34m);
    }
}
