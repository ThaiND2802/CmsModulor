using Commerce.Modules.Inventory.Contracts.Requests;
using Commerce.Modules.Inventory.Contracts.Responses;
using Commerce.Modules.Order.Contracts;
using Commerce.Modules.Sale.Application.Commands.SubmitSale;
using Commerce.Modules.Sale.Application.Services;
using Commerce.Modules.Sale.Domain;
using Commerce.Modules.Sale.Tests.TestCommon;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Commerce.Modules.Sale.Tests.Application.Commands.SubmitSale;

public sealed class SubmitSaleHandlerTests
{
    [Fact]
    public async Task Handle_SubmitsSaleAndStoresOrderId()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        await using var seedContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var sale = SaleTestFixture.CreatePricedSaleEntity();
        seedContext.Sales.Add(sale);
        await seedContext.SaveChangesAsync();

        await using var dbContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var inventoryModule = SaleTestFixture.CreateInventoryModule(
            SaleTestFixture.CreateInventoryAvailabilityResponse(
                Guid.NewGuid(),
                sale.Id,
                new InventoryAvailabilityItemResponse(Guid.NewGuid(), sale.Items.First().VariantId!.Value, "SKU-001", 2)));
        var orderResponse = new OrderCreationResponse(true, Guid.NewGuid(), "ORD-20260427-0001", null, null);
        var handler = new SubmitSaleHandler(
            new SaleSubmissionService(dbContext, inventoryModule),
            SaleTestFixture.CreateOrderModule(orderResponse),
            SaleTestFixture.CreateMapper());

        var response = await handler.Handle(new SubmitSaleCommand { SaleId = sale.Id }, CancellationToken.None);
        var updatedSale = await dbContext.Sales.Include(x => x.StatusHistory).SingleAsync(x => x.Id == sale.Id);

        response.Status.Should().Be(200);
        updatedSale.Status.Should().Be(SaleStatus.Submitted);
        updatedSale.OrderId.Should().Be(orderResponse.OrderId);
        updatedSale.StatusHistory.Should().Contain(x => x.ToStatus == SaleStatus.Submitted);
        await inventoryModule.Received(1).ReserveStockAsync(
            Arg.Is<ReserveStockRequest>(request => request.OrderId == sale.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReleasesInventoryWhenOrderCreationFails()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        await using var seedContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var sale = SaleTestFixture.CreatePricedSaleEntity();
        seedContext.Sales.Add(sale);
        await seedContext.SaveChangesAsync();

        await using var dbContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var inventoryModule = SaleTestFixture.CreateInventoryModule(
            SaleTestFixture.CreateInventoryAvailabilityResponse(
                Guid.NewGuid(),
                sale.Id,
                new InventoryAvailabilityItemResponse(Guid.NewGuid(), sale.Items.First().VariantId!.Value, "SKU-001", 2)),
            SaleTestFixture.CreateInventoryAvailabilityResponse(Guid.NewGuid(), sale.Id));
        var orderModule = SaleTestFixture.CreateOrderModule(
            new OrderCreationResponse(false, null, null, "ORDER_FAILED", "Order creation failed."));
        var handler = new SubmitSaleHandler(
            new SaleSubmissionService(dbContext, inventoryModule),
            orderModule,
            SaleTestFixture.CreateMapper());

        var act = () => handler.Handle(new SubmitSaleCommand { SaleId = sale.Id }, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleAppException>();

        var updatedSale = await dbContext.Sales.Include(x => x.StatusHistory).SingleAsync(x => x.Id == sale.Id);
        updatedSale.Status.Should().Be(SaleStatus.Priced);
        updatedSale.OrderId.Should().BeNull();
        await inventoryModule.Received(1).ReleaseStockAsync(
            Arg.Is<ReleaseStockRequest>(request => request.OrderId == sale.Id),
            Arg.Any<CancellationToken>());
    }
}
