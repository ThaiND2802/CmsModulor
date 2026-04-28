using Commerce.Modules.Inventory.Contracts.Requests;
using Commerce.Modules.Inventory.Contracts.Responses;
using Commerce.Modules.Order.Contracts;
using Commerce.Modules.Sale.Application.Commands.CancelSale;
using Commerce.Modules.Sale.Application.Commands.CreateSale;
using Commerce.Modules.Sale.Application.Commands.MarkSalePaid;
using Commerce.Modules.Sale.Application.Commands.RepriceSale;
using Commerce.Modules.Sale.Application.Commands.SubmitSale;
using Commerce.Modules.Sale.Application.Services;
using Commerce.Modules.Sale.Domain;
using Commerce.Modules.Sale.Tests.TestCommon;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using SaleEntity = Commerce.Modules.Sale.Domain.Sale;

namespace Commerce.Modules.Sale.Tests.Application.Commands;

public sealed class SaleMvpCertificationTests
{
    [Fact]
    public async Task CreateThenSubmit_CertifiesGoldenPath()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        var mapper = SaleTestFixture.CreateMapper();
        var createCommand = CreateReservableSaleCommand();

        await using var seedContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var createHandler = new CreateSaleHandler(seedContext, SaleTestFixture.CreateSalePricingService(), mapper);
        var created = await createHandler.Handle(createCommand, CancellationToken.None);

        await using var dbContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var saleId = created.Data!.Id;
        var inventoryModule = SaleTestFixture.CreateInventoryModule(
            CreateReserveResponseForSale(await dbContext.Sales.Include(x => x.Items).SingleAsync(x => x.Id == saleId)));
        var orderResponse = new OrderCreationResponse(true, Guid.NewGuid(), "ORD-CERT-001", null, null);

        await RepriceSaleAsync(dbContext, saleId, mapper);
        var submitHandler = new SubmitSaleHandler(
            new SaleSubmissionService(dbContext, inventoryModule),
            SaleTestFixture.CreateOrderModule(orderResponse),
            mapper);

        var response = await submitHandler.Handle(new SubmitSaleCommand { SaleId = saleId }, CancellationToken.None);
        var updatedSale = await dbContext.Sales.Include(x => x.StatusHistory).SingleAsync(x => x.Id == saleId);

        response.Status.Should().Be(200);
        updatedSale.Status.Should().Be(SaleStatus.Submitted);
        updatedSale.PaymentStatus.Should().Be(PaymentStatus.Unpaid);
        updatedSale.OrderId.Should().Be(orderResponse.OrderId);
        updatedSale.StatusHistory.Should().Contain(x => x.ToStatus == SaleStatus.Submitted);
        await inventoryModule.Received(1).ReserveStockAsync(
            Arg.Is<ReserveStockRequest>(request => request.OrderId == saleId),
            Arg.Any<CancellationToken>());
        await inventoryModule.DidNotReceive().ReleaseStockAsync(Arg.Any<ReleaseStockRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateThenPayThenSubmit_CertifiesGoldenPath()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        var mapper = SaleTestFixture.CreateMapper();
        var createCommand = CreateReservableSaleCommand();

        await using var seedContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var createHandler = new CreateSaleHandler(seedContext, SaleTestFixture.CreateSalePricingService(), mapper);
        var created = await createHandler.Handle(createCommand, CancellationToken.None);

        await using var dbContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var saleId = created.Data!.Id;
        var pricedSale = await RepriceSaleAsync(dbContext, saleId, mapper);
        var inventoryModule = SaleTestFixture.CreateInventoryModule(CreateReserveResponseForSale(pricedSale));
        var orderResponse = new OrderCreationResponse(true, Guid.NewGuid(), "ORD-CERT-002", null, null);

        var markPaidHandler = new MarkSalePaidHandler(dbContext);
        await markPaidHandler.Handle(new MarkSalePaidCommand(saleId, pricedSale.TotalAmount, "PAY-CERT-001"), CancellationToken.None);

        var submitHandler = new SubmitSaleHandler(
            new SaleSubmissionService(dbContext, inventoryModule),
            SaleTestFixture.CreateOrderModule(orderResponse),
            mapper);

        var response = await submitHandler.Handle(new SubmitSaleCommand { SaleId = saleId }, CancellationToken.None);
        var updatedSale = await dbContext.Sales.Include(x => x.StatusHistory).SingleAsync(x => x.Id == saleId);

        response.Status.Should().Be(200);
        updatedSale.Status.Should().Be(SaleStatus.Submitted);
        updatedSale.PaymentStatus.Should().Be(PaymentStatus.Paid);
        updatedSale.PaidAmount.Should().Be(pricedSale.TotalAmount);
        updatedSale.OrderId.Should().Be(orderResponse.OrderId);
        await inventoryModule.Received(1).ReserveStockAsync(
            Arg.Is<ReserveStockRequest>(request => request.OrderId == saleId),
            Arg.Any<CancellationToken>());
        await inventoryModule.DidNotReceive().ReleaseStockAsync(Arg.Any<ReleaseStockRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateThenCancel_CertifiesGoldenPath()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        var mapper = SaleTestFixture.CreateMapper();

        await using var seedContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var createHandler = new CreateSaleHandler(seedContext, SaleTestFixture.CreateSalePricingService(), mapper);
        var created = await createHandler.Handle(SaleTestFixture.CreateValidCreateSaleCommand(), CancellationToken.None);

        await using var dbContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var saleId = created.Data!.Id;

        await RepriceSaleAsync(dbContext, saleId, mapper);
        var inventoryModule = SaleTestFixture.CreateInventoryModule();
        var cancelHandler = new CancelSaleHandler(new SaleSubmissionService(dbContext, inventoryModule), mapper);

        var response = await cancelHandler.Handle(
            new CancelSaleCommand { SaleId = saleId, Reason = "MVP certification cancel path." },
            CancellationToken.None);
        var updatedSale = await dbContext.Sales.Include(x => x.StatusHistory).SingleAsync(x => x.Id == saleId);

        response.Status.Should().Be(200);
        updatedSale.Status.Should().Be(SaleStatus.Cancelled);
        updatedSale.PaymentStatus.Should().Be(PaymentStatus.Unpaid);
        updatedSale.OrderId.Should().BeNull();
        updatedSale.StatusHistory.Should().Contain(x => x.ToStatus == SaleStatus.Cancelled);
        await inventoryModule.DidNotReceive().ReserveStockAsync(Arg.Any<ReserveStockRequest>(), Arg.Any<CancellationToken>());
        await inventoryModule.DidNotReceive().ReleaseStockAsync(Arg.Any<ReleaseStockRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Submit_WhenOversellRaceLosesReservation_FailsCleanlyWithoutSideEffects()
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
        var updatedSale = await dbContext.Sales.SingleAsync(x => x.Id == sale.Id);
        updatedSale.Status.Should().Be(SaleStatus.Priced);
        updatedSale.PaymentStatus.Should().Be(PaymentStatus.Unpaid);
        updatedSale.OrderId.Should().BeNull();
        await orderModule.DidNotReceive().CreateOrderFromSaleAsync(Arg.Any<CreateOrderFromSaleRequest>(), Arg.Any<CancellationToken>());
        await inventoryModule.DidNotReceive().ReleaseStockAsync(Arg.Any<ReleaseStockRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SubmitTwice_CreatesSingleOrderAndSingleReservation()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        await using var seedContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var sale = SaleTestFixture.CreatePricedSaleEntity();
        seedContext.Sales.Add(sale);
        await seedContext.SaveChangesAsync();

        await using var dbContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var inventoryModule = SaleTestFixture.CreateInventoryModule(CreateReserveResponseForSale(sale));
        var orderResponse = new OrderCreationResponse(true, Guid.NewGuid(), "ORD-CERT-003", null, null);
        var orderModule = SaleTestFixture.CreateOrderModule(orderResponse);
        var handler = new SubmitSaleHandler(
            new SaleSubmissionService(dbContext, inventoryModule),
            orderModule,
            SaleTestFixture.CreateMapper());

        var firstResponse = await handler.Handle(new SubmitSaleCommand { SaleId = sale.Id }, CancellationToken.None);
        var secondResponse = await handler.Handle(new SubmitSaleCommand { SaleId = sale.Id }, CancellationToken.None);
        var updatedSale = await dbContext.Sales.Include(x => x.StatusHistory).SingleAsync(x => x.Id == sale.Id);

        firstResponse.Status.Should().Be(200);
        secondResponse.Status.Should().Be(200);
        secondResponse.Data!.Id.Should().Be(firstResponse.Data!.Id);
        secondResponse.Data.Status.Should().Be(SaleStatus.Submitted.ToString());
        updatedSale.OrderId.Should().Be(orderResponse.OrderId);
        updatedSale.StatusHistory.Count(x => x.ToStatus == SaleStatus.Submitted).Should().Be(1);
        await inventoryModule.Received(1).ReserveStockAsync(Arg.Any<ReserveStockRequest>(), Arg.Any<CancellationToken>());
        await orderModule.Received(1).CreateOrderFromSaleAsync(Arg.Any<CreateOrderFromSaleRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SubmitRetry_AfterSuccess_IsStableAndIdempotent()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        await using var seedContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var sale = SaleTestFixture.CreatePricedSaleEntity();
        seedContext.Sales.Add(sale);
        await seedContext.SaveChangesAsync();

        await using var dbContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var inventoryModule = SaleTestFixture.CreateInventoryModule(CreateReserveResponseForSale(sale));
        var orderResponse = new OrderCreationResponse(true, Guid.NewGuid(), "ORD-CERT-004", null, null);
        var orderModule = SaleTestFixture.CreateOrderModule(orderResponse);
        var handler = new SubmitSaleHandler(
            new SaleSubmissionService(dbContext, inventoryModule),
            orderModule,
            SaleTestFixture.CreateMapper());

        await handler.Handle(new SubmitSaleCommand { SaleId = sale.Id }, CancellationToken.None);
        var retryResponse = await handler.Handle(new SubmitSaleCommand { SaleId = sale.Id }, CancellationToken.None);
        var updatedSale = await dbContext.Sales.SingleAsync(x => x.Id == sale.Id);

        retryResponse.Status.Should().Be(200);
        updatedSale.Status.Should().Be(SaleStatus.Submitted);
        updatedSale.OrderId.Should().Be(orderResponse.OrderId);
        await inventoryModule.Received(1).ReserveStockAsync(Arg.Any<ReserveStockRequest>(), Arg.Any<CancellationToken>());
        await orderModule.Received(1).CreateOrderFromSaleAsync(Arg.Any<CreateOrderFromSaleRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Submit_WhenOrderCreationFails_RollsBackReservationCleanly()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        await using var seedContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var sale = SaleTestFixture.CreatePricedSaleEntity();
        seedContext.Sales.Add(sale);
        await seedContext.SaveChangesAsync();

        await using var dbContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var inventoryModule = SaleTestFixture.CreateInventoryModule(
            CreateReserveResponseForSale(sale),
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
        updatedSale.PaymentStatus.Should().Be(PaymentStatus.Unpaid);
        updatedSale.OrderId.Should().BeNull();
        updatedSale.StatusHistory.Should().NotContain(x => x.ToStatus == SaleStatus.Submitted);
        await inventoryModule.Received(1).ReserveStockAsync(Arg.Any<ReserveStockRequest>(), Arg.Any<CancellationToken>());
        await inventoryModule.Received(1).ReleaseStockAsync(
            Arg.Is<ReleaseStockRequest>(request => request.OrderId == sale.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SubmitRetry_AfterOrderCreationFailure_RollsBackAndCanSucceedCleanly()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        await using var seedContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var sale = SaleTestFixture.CreatePricedSaleEntity();
        seedContext.Sales.Add(sale);
        await seedContext.SaveChangesAsync();

        var inventoryModule = SaleTestFixture.CreateInventoryModule(
            CreateReserveResponseForSale(sale),
            SaleTestFixture.CreateInventoryAvailabilityResponse(Guid.NewGuid(), sale.Id));
        var successOrderId = Guid.NewGuid();
        var orderModule = SaleTestFixture.CreateOrderModule();
        orderModule
            .CreateOrderFromSaleAsync(Arg.Any<CreateOrderFromSaleRequest>(), Arg.Any<CancellationToken>())
            .Returns(
                new OrderCreationResponse(false, null, null, "ORDER_FAILED", "Order creation failed."),
                new OrderCreationResponse(true, successOrderId, "ORD-CERT-RETRY", null, null));

        await using (var firstAttemptContext = SaleTestFixture.CreateSaleDbContext(databaseName))
        {
            var firstAttemptHandler = new SubmitSaleHandler(
                new SaleSubmissionService(firstAttemptContext, inventoryModule),
                orderModule,
                SaleTestFixture.CreateMapper());

            var firstAct = () => firstAttemptHandler.Handle(new SubmitSaleCommand { SaleId = sale.Id }, CancellationToken.None);
            await firstAct.Should().ThrowAsync<BusinessRuleAppException>();
        }

        await using (var verificationContext = SaleTestFixture.CreateSaleDbContext(databaseName))
        {
            var failedSale = await verificationContext.Sales.Include(x => x.StatusHistory).SingleAsync(x => x.Id == sale.Id);
            failedSale.Status.Should().Be(SaleStatus.Priced);
            failedSale.PaymentStatus.Should().Be(PaymentStatus.Unpaid);
            failedSale.OrderId.Should().BeNull();
            failedSale.StatusHistory.Should().NotContain(x => x.ToStatus == SaleStatus.Submitted);
        }

        await inventoryModule.Received(1).ReserveStockAsync(Arg.Any<ReserveStockRequest>(), Arg.Any<CancellationToken>());
        await inventoryModule.Received(1).ReleaseStockAsync(
            Arg.Is<ReleaseStockRequest>(request => request.OrderId == sale.Id),
            Arg.Any<CancellationToken>());
        await orderModule.Received(1).CreateOrderFromSaleAsync(Arg.Any<CreateOrderFromSaleRequest>(), Arg.Any<CancellationToken>());

        await using (var retryContext = SaleTestFixture.CreateSaleDbContext(databaseName))
        {
            var retryHandler = new SubmitSaleHandler(
                new SaleSubmissionService(retryContext, inventoryModule),
                orderModule,
                SaleTestFixture.CreateMapper());

            var retryResponse = await retryHandler.Handle(new SubmitSaleCommand { SaleId = sale.Id }, CancellationToken.None);
            var retriedSale = await retryContext.Sales.Include(x => x.StatusHistory).SingleAsync(x => x.Id == sale.Id);

            retryResponse.Status.Should().Be(200);
            retryResponse.Data.Should().NotBeNull();
            retryResponse.Data!.Id.Should().Be(sale.Id);
            retryResponse.Data.Status.Should().Be(SaleStatus.Submitted.ToString());
            retriedSale.Status.Should().Be(SaleStatus.Submitted);
            retriedSale.PaymentStatus.Should().Be(PaymentStatus.Unpaid);
            retriedSale.OrderId.Should().Be(successOrderId);
            retriedSale.StatusHistory.Count(x => x.ToStatus == SaleStatus.Submitted).Should().Be(1);
        }

        await inventoryModule.Received(2).ReserveStockAsync(Arg.Any<ReserveStockRequest>(), Arg.Any<CancellationToken>());
        await inventoryModule.Received(1).ReleaseStockAsync(
            Arg.Is<ReleaseStockRequest>(request => request.OrderId == sale.Id),
            Arg.Any<CancellationToken>());
        await orderModule.Received(2).CreateOrderFromSaleAsync(Arg.Any<CreateOrderFromSaleRequest>(), Arg.Any<CancellationToken>());
    }

    private static async Task<SaleEntity> RepriceSaleAsync(DbContext dbContext, Guid saleId, AutoMapper.IMapper mapper)
    {
        var saleDbContext = (Commerce.Modules.Sale.Infrastructure.SaleDbContext)dbContext;
        var handler = new RepriceSaleHandler(
            saleDbContext,
            SaleTestFixture.CreateSalePricingService(),
            mapper);

        await handler.Handle(new RepriceSaleCommand
        {
            SaleId = saleId,
            DiscountAmount = 2m,
            ShippingAmount = 5m,
            TaxAmount = 1m
        }, CancellationToken.None);

        return await saleDbContext.Sales.Include(x => x.Items).SingleAsync(x => x.Id == saleId);
    }

    private static InventoryAvailabilityResponse CreateReserveResponseForSale(SaleEntity sale)
    {
        var variantItems = sale.Items
            .Where(static item => item.VariantId.HasValue)
            .Select(item => new InventoryAvailabilityItemResponse(
                Guid.NewGuid(),
                item.VariantId!.Value,
                item.ProductSku,
                item.Quantity))
            .ToArray();

        return SaleTestFixture.CreateInventoryAvailabilityResponse(Guid.NewGuid(), sale.Id, variantItems);
    }

    private static CreateSaleCommand CreateReservableSaleCommand()
    {
        var command = SaleTestFixture.CreateValidCreateSaleCommand();
        return command with
        {
            Items =
            [
                command.Items[0] with
                {
                    VariantId = Guid.NewGuid(),
                    VariantName = "Default"
                }
            ]
        };
    }
}
