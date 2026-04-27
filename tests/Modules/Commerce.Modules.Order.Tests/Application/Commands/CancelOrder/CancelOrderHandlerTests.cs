using Commerce.Modules.Inventory.Contracts;
using Commerce.Modules.Inventory.Contracts.Requests;
using Commerce.Modules.Inventory.Contracts.Responses;
using Commerce.Modules.Order.Application.Commands.CancelOrder;
using Commerce.Modules.Order.Application.Events;
using Commerce.Modules.Order.Domain;
using Commerce.Modules.Order.Infrastructure;
using Commerce.Modules.Order.Tests.TestCommon;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;
using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using OrderEntity = Commerce.Modules.Order.Domain.Order;

namespace Commerce.Modules.Order.Tests.Application.Commands.CancelOrder;

public sealed class CancelOrderHandlerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly OrderDbContext _dbContext;
    private readonly IMediator _mediator;

    public CancelOrderHandlerTests()
    {
        _connection = OrderTestFixture.CreateSharedInMemoryConnection();
        _dbContext = OrderTestFixture.CreateInMemoryDbContext(_connection, ensureCreated: true);
        _mediator = Substitute.For<IMediator>();
    }

    private OrderDbContext CreateFreshContext() =>
        OrderTestFixture.CreateInMemoryDbContext(_connection);

    private CancelOrderHandler CreateHandler(OrderDbContext dbContext, IInventoryModule inventoryModule) =>
        new(dbContext, inventoryModule, OrderTestFixture.CreateMapper(), _mediator);


    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }

    private async Task<OrderEntity> SeedOrderAsync(OrderStatus status, params OrderItem[] items)
    {
        var order = OrderTestFixture.CreateOrderEntity(status, items: items);
        await _dbContext.Orders.AddAsync(order);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
        return order;
    }

    [Fact]
    public async Task Handle_PendingOrder_CancelsSuccessfully()
    {
        var order = await SeedOrderAsync(OrderStatus.Pending);
        var inventoryModule = OrderTestFixture.CreateInventoryModule();

        var handler = CreateHandler(_dbContext, inventoryModule);

        var response = await handler.Handle(
            new CancelOrderCommand
            {
                Id = order.Id,
                CancelReason = "  Customer request  "
            },
            CancellationToken.None);

        response.Status.Should().Be(200);

        await using var assertionContext = CreateFreshContext();
        var updatedOrder = await assertionContext.Orders.Include(o => o.StatusHistory).FirstAsync(o => o.Id == order.Id);
        updatedOrder.Status.Should().Be(OrderStatus.Cancelled);
        updatedOrder.CancelReason.Should().Be("Customer request");
        updatedOrder.StatusHistory.Should().HaveCount(1);

        var history = updatedOrder.StatusHistory.Single();
        history.ToStatus.Should().Be(OrderStatus.Cancelled);
        history.FromStatus.Should().Be(OrderStatus.Pending);
        history.Note.Should().Be("Customer request");
        response.Data!.Status.Should().Be(nameof(OrderStatus.Cancelled));
        response.Data.CancelReason.Should().Be("Customer request");
        response.Data.StatusHistory.Should().HaveCount(1);

        await inventoryModule.DidNotReceive().ReleaseStockAsync(Arg.Any<ReleaseStockRequest>(), Arg.Any<CancellationToken>());
        await _mediator.Received(1).Publish(
            Arg.Is<OrderCancelled>(x => x.OrderId == order.Id && x.CancelReason == "Customer request"),
            CancellationToken.None);
        await _mediator.Received(1).Publish(
            Arg.Is<OrderStatusChanged>(x => x.OrderId == order.Id && x.FromStatus == OrderStatus.Pending && x.ToStatus == OrderStatus.Cancelled),
            CancellationToken.None);
    }

    [Fact]
    public async Task Handle_ConfirmedOrder_ReleasesStockBeforeCancelling()
    {
        var variantId = Guid.NewGuid();
        var order = await SeedOrderAsync(OrderStatus.Confirmed, OrderTestFixture.CreateOrderItem(variantId, 2));
        var inventoryModule = OrderTestFixture.CreateInventoryModule(
            releaseResponse: OrderTestFixture.CreateInventoryAvailabilityResponse(
                order.Id,
                new InventoryAvailabilityItemResponse(Guid.NewGuid(), variantId, "SKU-001", 2)));

        var handler = CreateHandler(_dbContext, inventoryModule);

        var response = await handler.Handle(
            new CancelOrderCommand
            {
                Id = order.Id,
                CancelReason = "Customer request"
            },
            CancellationToken.None);

        response.Status.Should().Be(200);
        await inventoryModule.Received(1).ReleaseStockAsync(
            Arg.Is<ReleaseStockRequest>(x => x.OrderId == order.Id),
            CancellationToken.None);
    }

    [Fact]
    public async Task Handle_AlreadyCancelledOrder_IsIdempotent()
    {
        var order = await SeedOrderAsync(OrderStatus.Cancelled);
        var inventoryModule = OrderTestFixture.CreateInventoryModule();

        var handler = CreateHandler(_dbContext, inventoryModule);

        var response = await handler.Handle(
            new CancelOrderCommand
            {
                Id = order.Id,
                CancelReason = "Customer request"
            },
            CancellationToken.None);

        response.Status.Should().Be(200);
        response.Data!.Status.Should().Be(nameof(OrderStatus.Cancelled));

        await using var assertionContext = CreateFreshContext();
        var updatedOrder = await assertionContext.Orders.Include(o => o.StatusHistory).FirstAsync(o => o.Id == order.Id);
        updatedOrder.StatusHistory.Should().BeEmpty();

        await inventoryModule.DidNotReceive().ReleaseStockAsync(Arg.Any<ReleaseStockRequest>(), Arg.Any<CancellationToken>());
        await _mediator.DidNotReceive().Publish(Arg.Any<OrderCancelled>(), Arg.Any<CancellationToken>());
        await _mediator.DidNotReceive().Publish(Arg.Any<OrderStatusChanged>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShippedOrder_ThrowsBusinessRuleException()
    {
        var order = await SeedOrderAsync(OrderStatus.Shipped);
        var inventoryModule = OrderTestFixture.CreateInventoryModule();

        var handler = CreateHandler(_dbContext, inventoryModule);

        var act = async () => await handler.Handle(
            new CancelOrderCommand
            {
                Id = order.Id,
                CancelReason = "Customer request"
            },
            CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleAppException>()
            .WithMessage("Only pending, confirmed, or processing orders can be cancelled.");
    }
}
