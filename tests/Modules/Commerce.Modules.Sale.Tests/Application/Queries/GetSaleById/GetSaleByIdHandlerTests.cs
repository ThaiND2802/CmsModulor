using Commerce.Modules.Sale.Application.Queries.GetSaleById;
using Commerce.Modules.Sale.Domain;
using Commerce.Modules.Sale.Tests.TestCommon;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Sale.Tests.Application.Queries.GetSaleById;

public sealed class GetSaleByIdHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsSaleDetails()
    {
        await using var dbContext = SaleTestFixture.CreateSaleDbContext();
        var mapper = SaleTestFixture.CreateMapper();
        var sale = SaleTestFixture.CreateSaleEntity();
        sale.Items.Add(new SaleItem
        {
            Id = Guid.NewGuid(),
            ProductName = "Product",
            ProductSku = "SKU-1",
            UnitPrice = 10m,
            Quantity = 2,
            DiscountAmount = 1m,
            TotalAmount = 19m
        });
        dbContext.Sales.Add(sale);
        await dbContext.SaveChangesAsync();

        var handler = new GetSaleByIdHandler(dbContext, mapper);
        var response = await handler.Handle(new GetSaleByIdQuery(sale.Id), CancellationToken.None);

        response.SaleNumber.Should().Be(sale.SaleNumber);
        response.Items.Should().ContainSingle();
        response.Totals.TotalAmount.Should().Be(23m);
    }
}
