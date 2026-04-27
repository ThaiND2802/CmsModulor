using Commerce.Modules.Sale.Application.Commands.CreateSale;
using Commerce.Modules.Sale.Domain;
using Commerce.Modules.Sale.Tests.TestCommon;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Sale.Tests.Application.Commands.CreateSale;

public sealed class CreateSaleHandlerTests
{
    [Fact]
    public async Task Handle_CreatesDraftSale()
    {
        await using var dbContext = SaleTestFixture.CreateSaleDbContext();
        var mapper = SaleTestFixture.CreateMapper();
        var handler = new CreateSaleHandler(dbContext, SaleTestFixture.CreateSalePricingService(), mapper);
        var command = SaleTestFixture.CreateValidCreateSaleCommand();

        var response = await handler.Handle(command, CancellationToken.None);
        var sale = await dbContext.Sales.Include(x => x.Items).Include(x => x.StatusHistory).SingleAsync(x => x.Id == response.Data!.Id);

        response.Status.Should().Be(201);
        sale.Status.Should().Be(SaleStatus.Draft);
        sale.Currency.Should().Be("USD");
        sale.Items.Should().ContainSingle();
        sale.StatusHistory.Should().ContainSingle(x => x.ToStatus == SaleStatus.Draft);
    }
}
