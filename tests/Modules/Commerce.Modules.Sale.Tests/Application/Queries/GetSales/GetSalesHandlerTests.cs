using Commerce.Modules.Sale.Application.Queries.GetSales;
using Commerce.Modules.Sale.Domain;
using Commerce.Modules.Sale.Tests.TestCommon;
using FluentAssertions;

namespace Commerce.Modules.Sale.Tests.Application.Queries.GetSales;

public sealed class GetSalesHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsPagedSales()
    {
        await using var dbContext = SaleTestFixture.CreateSaleDbContext();
        var mapper = SaleTestFixture.CreateMapper();
        var firstSale = SaleTestFixture.CreateSaleEntity(SaleStatus.Draft, "SAL-20260427-0001");
        var secondSale = SaleTestFixture.CreateSaleEntity(SaleStatus.Submitted, "SAL-20260427-0002");
        dbContext.Sales.AddRange(firstSale, secondSale);
        await dbContext.SaveChangesAsync();

        var handler = new GetSalesHandler(dbContext, mapper);
        var response = await handler.Handle(new GetSalesQuery { Page = 1, PageSize = 10, Status = SaleStatus.Draft }, CancellationToken.None);

        response.Data.Should().ContainSingle();
        response.Data.Single().SaleNumber.Should().Be("SAL-20260427-0001");
        response.Pagination.Total.Should().Be(1);
    }
}
