using Commerce.Modules.Inventory.Contracts.Requests;
using Commerce.Modules.Order.Application.Commands.CancelOrder;
using Commerce.Modules.Order.Application.Commands.ConfirmOrder;
using Commerce.Modules.Order.Application.Commands.CreateOrder;
using Commerce.Modules.Order.Application.Queries.GetOrderById;
using Commerce.Modules.Order.Application.Queries.GetOrders;
using Commerce.Modules.Order.Domain;
using Commerce.Modules.Order.Infrastructure;
using Commerce.Modules.Order.Tests.TestCommon;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Commerce.Modules.Order.Tests.Integration;

[CollectionDefinition("OrderIntegration")]
public sealed class OrderIntegrationCollection : ICollectionFixture<OrderIntegrationFixture>;

[Collection("OrderIntegration")]
public sealed class OrderIntegrationTests : IAsyncLifetime
{
    private readonly OrderIntegrationFixture _fixture;
    private OrderDbContext _dbContext = default!;

    public OrderIntegrationTests(OrderIntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await _fixture.ResetAsync();
        _dbContext = _fixture.CreateDbContext();
    }

    public Task DisposeAsync()
    {
        _dbContext.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task CreateOrder_FetchById_AllFieldsMatch()
    {
        var mapper = OrderTestFixture.CreateMapper();
        var mediator = Substitute.For<IMediator>();
        var createHandler = new CreateOrderHandler(_dbContext, mapper, mediator);
        var createResult = await createHandler.Handle(
            OrderTestFixture.CreateValidCreateOrderCommand() with
            {
                CustomerEmail = "integration@test.com",
                TaxAmount = 2m,
                ShippingAmount = 3m,
                Notes = "  Handle with care  "
            },
            CancellationToken.None);

        await using var readContext = _fixture.CreateDbContext();
        var getHandler = new GetOrderByIdHandler(readContext, mapper);
        var getResult = await getHandler.Handle(new GetOrderByIdQuery { Id = createResult.Data!.Id }, CancellationToken.None);

        getResult.Status.Should().Be(200);
        getResult.Data!.CustomerEmail.Should().Be("integration@test.com");
        getResult.Data.TotalAmount.Should().Be(195m);
        getResult.Data.Notes.Should().Be("Handle with care");
        getResult.Data.Items.Should().HaveCount(1);
        getResult.Data.Status.Should().Be(nameof(OrderStatus.Pending));
        getResult.Data.StatusHistory.Should().HaveCount(1);
        getResult.Data.StatusHistory.Single().ToStatus.Should().Be(nameof(OrderStatus.Pending));
    }

    [Fact]
    public async Task CreateOrder_Confirm_StatusHistoryHasTwoEntries()
    {
        var mapper = OrderTestFixture.CreateMapper();
        var mediator = Substitute.For<IMediator>();
        var inventoryModule = OrderTestFixture.CreateInventoryModule();
        var createHandler = new CreateOrderHandler(_dbContext, mapper, mediator);
        var confirmHandler = new ConfirmOrderHandler(_dbContext, inventoryModule, mapper, mediator);

        var createResult = await createHandler.Handle(
            OrderTestFixture.CreateValidCreateOrderCommand() with
            {
                Items =
                [
                    new CreateOrderItemRequest(
                        ProductId: Guid.NewGuid(),
                        ProductName: "Test Product",
                        ProductSku: "TEST-SKU",
                        VariantId: Guid.NewGuid(),
                        VariantName: "Variant",
                        UnitPrice: 100m,
                        Quantity: 2,
                        DiscountAmount: 10m)
                ]
            },
            CancellationToken.None);
        var orderId = createResult.Data!.Id;

        var confirmResult = await confirmHandler.Handle(
            new ConfirmOrderCommand { Id = orderId, Note = "  All good  " },
            CancellationToken.None);

        confirmResult.Status.Should().Be(200);
        confirmResult.Data!.Status.Should().Be(nameof(OrderStatus.Confirmed));
        confirmResult.Data.StatusHistory.Should().HaveCount(2);
        confirmResult.Data.StatusHistory.Last().ToStatus.Should().Be(nameof(OrderStatus.Confirmed));
        confirmResult.Data.StatusHistory.Last().FromStatus.Should().Be(nameof(OrderStatus.Pending));
        confirmResult.Data.StatusHistory.Last().Note.Should().Be("All good");

        await using var assertionContext = _fixture.CreateDbContext();
        var order = await assertionContext.Orders.Include(static o => o.StatusHistory).FirstAsync(o => o.Id == orderId);
        order.Status.Should().Be(OrderStatus.Confirmed);
        order.StatusHistory.Should().HaveCount(2);
        order.StatusHistory.OrderBy(static x => x.ChangedAtUtc).Last().ToStatus.Should().Be(OrderStatus.Confirmed);

        await inventoryModule.Received(1).ReserveStockAsync(
            Arg.Is<ReserveStockRequest>(x => x.OrderId == orderId && x.Items.Count == 1 && x.Items[0].Quantity == 2),
            CancellationToken.None);
    }

    [Fact]
    public async Task CreateOrder_Cancel_CancelReasonPersisted()
    {
        var mapper = OrderTestFixture.CreateMapper();
        var mediator = Substitute.For<IMediator>();
        var inventoryModule = OrderTestFixture.CreateInventoryModule();
        var createHandler = new CreateOrderHandler(_dbContext, mapper, mediator);
        var confirmHandler = new ConfirmOrderHandler(_dbContext, inventoryModule, mapper, mediator);
        var cancelHandler = new CancelOrderHandler(_dbContext, inventoryModule, mapper, mediator);

        var createResult = await createHandler.Handle(
            OrderTestFixture.CreateValidCreateOrderCommand() with
            {
                Items =
                [
                    new CreateOrderItemRequest(
                        ProductId: Guid.NewGuid(),
                        ProductName: "Test Product",
                        ProductSku: "TEST-SKU",
                        VariantId: Guid.NewGuid(),
                        VariantName: "Variant",
                        UnitPrice: 100m,
                        Quantity: 2,
                        DiscountAmount: 10m)
                ]
            },
            CancellationToken.None);
        var orderId = createResult.Data!.Id;

        await confirmHandler.Handle(new ConfirmOrderCommand { Id = orderId }, CancellationToken.None);

        var cancelResult = await cancelHandler.Handle(
            new CancelOrderCommand { Id = orderId, CancelReason = "  Changed mind  " },
            CancellationToken.None);

        cancelResult.Status.Should().Be(200);
        cancelResult.Data!.CancelReason.Should().Be("Changed mind");
        cancelResult.Data.Status.Should().Be(nameof(OrderStatus.Cancelled));

        await using var assertionContext = _fixture.CreateDbContext();
        var order = await assertionContext.Orders.Include(static o => o.StatusHistory).FirstAsync(o => o.Id == orderId);
        order.Status.Should().Be(OrderStatus.Cancelled);
        order.CancelReason.Should().Be("Changed mind");
        order.StatusHistory.Should().HaveCount(3);

        await inventoryModule.Received(1).ReleaseStockAsync(
            Arg.Is<ReleaseStockRequest>(x => x.OrderId == orderId),
            CancellationToken.None);
    }

    [Fact]
    public async Task SoftDeletedOrder_NotReturnedInQueries()
    {
        var order = OrderTestFixture.CreateOrderEntity(
            orderNumber: "DEL-123",
            customerEmail: "del@test.com",
            createdAtUtc: new DateTime(2026, 4, 25, 12, 0, 0, DateTimeKind.Utc));

        await _dbContext.Orders.AddAsync(order);
        await _dbContext.SaveChangesAsync();

        _dbContext.Orders.Remove(order);
        await _dbContext.SaveChangesAsync();

        var handler = new GetOrdersHandler(_dbContext, OrderTestFixture.CreateMapper());
        var result = await handler.Handle(new GetOrdersQuery(), CancellationToken.None);

        result.Data.Should().BeEmpty();

        await using var rawContext = _fixture.CreateDbContext();
        var storedOrder = await rawContext.Orders.IgnoreQueryFilters().FirstAsync(static o => o.OrderNumber == "DEL-123");
        storedOrder.IsDeleted.Should().BeTrue();
    }
}
