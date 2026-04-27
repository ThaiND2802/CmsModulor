using Commerce.Modules.Sale.Application.Commands.AddSaleItem;
using Commerce.Modules.Sale.Domain;
using Commerce.Modules.Sale.Tests.TestCommon;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Sale.Tests.Application.Commands.AddSaleItem;

public sealed class AddSaleItemHandlerTests
{
    [Fact]
    public async Task Handle_AddsItemAndRecalculatesTotals()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        await using var seedContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var sale = SaleTestFixture.CreateSaleEntity();
        seedContext.Sales.Add(sale);
        await seedContext.SaveChangesAsync();

        await using var dbContext = SaleTestFixture.CreateSaleDbContext(databaseName);

        var mapper = SaleTestFixture.CreateMapper();
        var variant = SaleTestFixture.CreateCatalogVariantSummary(unitPrice: 25m, productName: "Catalog Product", variantName: "Blue / M");
        var handler = new AddSaleItemHandler(
            dbContext,
            SaleTestFixture.CreateCatalogVariantLookup(variant),
            SaleTestFixture.CreateSalePricingService(),
            mapper);

        var response = await handler.Handle(new AddSaleItemCommand
        {
            SaleId = sale.Id,
            VariantId = variant.Id,
            Quantity = 2,
            DiscountAmount = 5m
        }, CancellationToken.None);

        var updatedSale = await dbContext.Sales.Include(x => x.Items).SingleAsync(x => x.Id == sale.Id);
        var item = updatedSale.Items.Single();
        response.Status.Should().Be(200);
        updatedSale.Items.Should().ContainSingle();
        item.ProductName.Should().Be("Catalog Product");
        item.VariantName.Should().Be("Blue / M");
        item.TotalAmount.Should().Be(45m);
        updatedSale.SubtotalAmount.Should().Be(50m);
        updatedSale.TotalAmount.Should().Be(49m);
    }

    [Fact]
    public async Task Handle_Throws_WhenSaleIsNotMutable()
    {
        await using var dbContext = SaleTestFixture.CreateSaleDbContext();
        var sale = SaleTestFixture.CreateSaleEntity(SaleStatus.Submitted);
        dbContext.Sales.Add(sale);
        await dbContext.SaveChangesAsync();

        var variant = SaleTestFixture.CreateCatalogVariantSummary();
        var handler = new AddSaleItemHandler(
            dbContext,
            SaleTestFixture.CreateCatalogVariantLookup(variant),
            SaleTestFixture.CreateSalePricingService(),
            SaleTestFixture.CreateMapper());

        var act = () => handler.Handle(new AddSaleItemCommand
        {
            SaleId = sale.Id,
            VariantId = variant.Id,
            Quantity = 1
        }, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationAppException>();
    }
}
