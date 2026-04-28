using Commerce.Modules.Inventory.Contracts.Requests;
using Commerce.Modules.Inventory.Contracts.Responses;
using Commerce.Modules.Order.Contracts;
using Commerce.Modules.Sale.Application.Commands.SubmitSale;
using Commerce.Modules.Sale.Application.Commands.ValidateSaleStock;
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
        updatedSale.PaymentStatus.Should().Be(PaymentStatus.Unpaid);
        updatedSale.PaidAmount.Should().Be(0m);
        updatedSale.PaymentReference.Should().BeNull();
        updatedSale.PaidAtUtc.Should().BeNull();
        updatedSale.OrderId.Should().Be(orderResponse.OrderId);
        updatedSale.StatusHistory.Should().Contain(x => x.ToStatus == SaleStatus.Submitted);
        await inventoryModule.Received(1).ReserveStockAsync(
            Arg.Is<ReserveStockRequest>(request => request.OrderId == sale.Id),
            Arg.Any<CancellationToken>());
        await inventoryModule.DidNotReceive().ReleaseStockAsync(Arg.Any<ReleaseStockRequest>(), Arg.Any<CancellationToken>());
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

    [Fact]
    public async Task Handle_RepeatedSubmit_ReturnsSameOutcomeWithoutDuplicateSideEffects()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        var orderId = Guid.NewGuid();

        await using (var seedContext = SaleTestFixture.CreateSaleDbContext(databaseName))
        {
            var sale = SaleTestFixture.CreatePricedSaleEntity();
            sale.Status = SaleStatus.Submitted;
            sale.OrderId = orderId;
            sale.SubmittedAtUtc = new DateTime(2026, 4, 27, 1, 0, 0, DateTimeKind.Utc);
            sale.StatusHistory.Add(new SaleStatusHistory
            {
                Id = Guid.NewGuid(),
                SaleId = sale.Id,
                FromStatus = SaleStatus.Priced,
                ToStatus = SaleStatus.Submitted,
                ChangedAtUtc = sale.SubmittedAtUtc.Value
            });

            seedContext.Sales.Add(sale);
            await seedContext.SaveChangesAsync();
        }

        await using var dbContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var inventoryModule = SaleTestFixture.CreateInventoryModule();
        var orderModule = SaleTestFixture.CreateOrderModule();
        var handler = new SubmitSaleHandler(
            new SaleSubmissionService(dbContext, inventoryModule),
            orderModule,
            SaleTestFixture.CreateMapper());

        var saleId = await dbContext.Sales.Select(static x => x.Id).SingleAsync();
        var response = await handler.Handle(new SubmitSaleCommand { SaleId = saleId }, CancellationToken.None);

        response.Status.Should().Be(200);
        response.Data.Should().NotBeNull();
        response.Data!.Id.Should().Be(saleId);
        response.Data.Status.Should().Be(SaleStatus.Submitted.ToString());

        var updatedSale = await dbContext.Sales.Include(x => x.StatusHistory).SingleAsync(x => x.Id == saleId);
        updatedSale.OrderId.Should().Be(orderId);
        updatedSale.StatusHistory.Count(x => x.ToStatus == SaleStatus.Submitted).Should().Be(1);
        await inventoryModule.DidNotReceive().ReserveStockAsync(Arg.Any<ReserveStockRequest>(), Arg.Any<CancellationToken>());
        await inventoryModule.DidNotReceive().ReleaseStockAsync(Arg.Any<ReleaseStockRequest>(), Arg.Any<CancellationToken>());
        await orderModule.DidNotReceive().CreateOrderFromSaleAsync(Arg.Any<CreateOrderFromSaleRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AfterValidateStock_ReusesReservationAndCreatesOrderOnce()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        await using var seedContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var sale = SaleTestFixture.CreatePricedSaleEntity();
        seedContext.Sales.Add(sale);
        await seedContext.SaveChangesAsync();

        await using var dbContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var reservationId = Guid.NewGuid();
        var inventoryModule = SaleTestFixture.CreateInventoryModule(
            SaleTestFixture.CreateInventoryAvailabilityResponse(
                reservationId,
                sale.Id,
                new InventoryAvailabilityItemResponse(Guid.NewGuid(), sale.Items.First().VariantId!.Value, "SKU-001", 2)));
        var orderResponse = new OrderCreationResponse(true, Guid.NewGuid(), "ORD-20260427-RETRY", null, null);
        var saleSubmissionService = new SaleSubmissionService(dbContext, inventoryModule);

        var validateHandler = new ValidateSaleStockHandler(saleSubmissionService);
        var validateResponse = await validateHandler.Handle(new ValidateSaleStockCommand { SaleId = sale.Id }, CancellationToken.None);

        validateResponse.Data.Should().NotBeNull();
        validateResponse.Data!.Guaranteed.Should().BeFalse();
        validateResponse.Data.Mode.Should().Be("preview_only");
        validateResponse.Data.ReservationReference.Should().Be(reservationId.ToString());

        var submitHandler = new SubmitSaleHandler(
            saleSubmissionService,
            SaleTestFixture.CreateOrderModule(orderResponse),
            SaleTestFixture.CreateMapper());

        var submitResponse = await submitHandler.Handle(new SubmitSaleCommand { SaleId = sale.Id }, CancellationToken.None);
        var updatedSale = await dbContext.Sales.SingleAsync(x => x.Id == sale.Id);

        submitResponse.Status.Should().Be(200);
        updatedSale.Status.Should().Be(SaleStatus.Submitted);
        updatedSale.OrderId.Should().Be(orderResponse.OrderId);
        await inventoryModule.Received(2).ReserveStockAsync(
            Arg.Is<ReserveStockRequest>(request => request.OrderId == sale.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenStockChangesAfterPreview_FailsBeforeCreatingOrder()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        await using var seedContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var sale = SaleTestFixture.CreatePricedSaleEntity();
        seedContext.Sales.Add(sale);
        await seedContext.SaveChangesAsync();

        await using var dbContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var inventoryModule = SaleTestFixture.CreateInventoryModule();
        inventoryModule
            .ReserveStockAsync(Arg.Any<ReserveStockRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<InventoryAvailabilityResponse?>(new ConflictAppException("Insufficient available stock.")));

        var orderModule = SaleTestFixture.CreateOrderModule();
        var handler = new SubmitSaleHandler(
            new SaleSubmissionService(dbContext, inventoryModule),
            orderModule,
            SaleTestFixture.CreateMapper());

        var act = () => handler.Handle(new SubmitSaleCommand { SaleId = sale.Id }, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictAppException>();
        await orderModule.DidNotReceive().CreateOrderFromSaleAsync(Arg.Any<CreateOrderFromSaleRequest>(), Arg.Any<CancellationToken>());
        await inventoryModule.DidNotReceive().ReleaseStockAsync(Arg.Any<ReleaseStockRequest>(), Arg.Any<CancellationToken>());

        var updatedSale = await dbContext.Sales.SingleAsync(x => x.Id == sale.Id);
        updatedSale.Status.Should().Be(SaleStatus.Priced);
        updatedSale.OrderId.Should().BeNull();
    }

    [Theory]
    [InlineData(SaleStatus.Cancelled)]
    [InlineData(SaleStatus.Expired)]
    public async Task Handle_InvalidTerminalState_RejectsSubmit(SaleStatus status)
    {
        var databaseName = Guid.NewGuid().ToString("N");
        await using var seedContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var sale = SaleTestFixture.CreateSaleEntity(status);
        sale.Items.Add(new SaleItem
        {
            Id = Guid.NewGuid(),
            SaleId = sale.Id,
            ProductId = Guid.NewGuid(),
            ProductName = "Product",
            ProductSku = "SKU-001",
            VariantId = Guid.NewGuid(),
            VariantName = "Default",
            UnitPrice = 10m,
            Quantity = 1,
            DiscountAmount = 0m,
            TotalAmount = 10m
        });
        seedContext.Sales.Add(sale);
        await seedContext.SaveChangesAsync();

        await using var dbContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var inventoryModule = SaleTestFixture.CreateInventoryModule();
        var orderModule = SaleTestFixture.CreateOrderModule();
        var handler = new SubmitSaleHandler(
            new SaleSubmissionService(dbContext, inventoryModule),
            orderModule,
            SaleTestFixture.CreateMapper());

        var act = () => handler.Handle(new SubmitSaleCommand { SaleId = sale.Id }, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleAppException>();
        await inventoryModule.DidNotReceive().ReserveStockAsync(Arg.Any<ReserveStockRequest>(), Arg.Any<CancellationToken>());
        await orderModule.DidNotReceive().CreateOrderFromSaleAsync(Arg.Any<CreateOrderFromSaleRequest>(), Arg.Any<CancellationToken>());
    }
}
