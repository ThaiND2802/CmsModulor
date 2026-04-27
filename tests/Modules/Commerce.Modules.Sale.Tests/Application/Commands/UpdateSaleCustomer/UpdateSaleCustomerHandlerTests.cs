using Commerce.Modules.Sale.Application.Commands.UpdateSaleCustomer;
using Commerce.Modules.Sale.Domain;
using Commerce.Modules.Sale.Tests.TestCommon;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Sale.Tests.Application.Commands.UpdateSaleCustomer;

public sealed class UpdateSaleCustomerHandlerTests
{
    [Fact]
    public async Task Handle_UpdatesCustomer_WhenSaleIsDraft()
    {
        await using var dbContext = SaleTestFixture.CreateSaleDbContext();
        var sale = SaleTestFixture.CreateSaleEntity();
        dbContext.Sales.Add(sale);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateSaleCustomerHandler(dbContext);
        await handler.Handle(new UpdateSaleCustomerCommand
        {
            Id = sale.Id,
            CustomerId = Guid.NewGuid(),
            CustomerEmail = "updated@example.com",
            CustomerPhone = "999"
        }, CancellationToken.None);

        var updated = await dbContext.Sales.SingleAsync(x => x.Id == sale.Id);
        updated.CustomerEmail.Should().Be("updated@example.com");
        updated.CustomerPhone.Should().Be("999");
    }

    [Fact]
    public async Task Handle_Throws_WhenSaleIsNotMutable()
    {
        await using var dbContext = SaleTestFixture.CreateSaleDbContext();
        var sale = SaleTestFixture.CreateSaleEntity(SaleStatus.Submitted);
        dbContext.Sales.Add(sale);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateSaleCustomerHandler(dbContext);
        var act = () => handler.Handle(new UpdateSaleCustomerCommand
        {
            Id = sale.Id,
            CustomerEmail = "updated@example.com"
        }, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationAppException>();
    }
}
