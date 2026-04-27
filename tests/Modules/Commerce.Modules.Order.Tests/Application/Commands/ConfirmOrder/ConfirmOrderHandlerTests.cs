using Commerce.Modules.Inventory.Contracts;
using Commerce.Modules.Inventory.Contracts.Requests;
using Commerce.Modules.Inventory.Contracts.Responses;
using Commerce.Modules.Order.Application.Commands.ConfirmOrder;
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

namespace Commerce.Modules.Order.Tests.Application.Commands.ConfirmOrder;

public sealed class ConfirmOrderHandlerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly OrderDbContext _dbContext;
    private readonly IMediator _mediator;

    public ConfirmOrderHandlerTests()
    {
        _connection = OrderTestFixture.CreateSharedInMemoryConnection();
        _dbContext = OrderTestFixture.CreateInMemoryDbContext(_connection, ensureCreated: true);
        _mediator = Substitute.For<IMediator>();
    }

    private OrderDbContext CreateFreshContext() =>
        OrderTestFixture.CreateInMemoryDbContext(_connection);

    private ConfirmOrderHandler CreateHandler(OrderDbContext dbContext, IInventoryModule inventoryModule) =>
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
    public async Task Handle_PendingOrderWithStock_ReservesAndConfirms()
    {
        var variantId = Guid.NewGuid();
        var order = await SeedOrderAsync(OrderStatus.Pending, OrderTestFixture.CreateOrderItem(variantId, 2));
        var inventoryModule = OrderTestFixture.CreateInventoryModule(
            reserveResponse: OrderTestFixture.CreateInventoryAvailabilityResponse(
                order.Id,
                new InventoryAvailabilityItemResponse(Guid.NewGuid(), variantId, "SKU-001", 2)));

        var handler = CreateHandler(_dbContext, inventoryModule);

        var response = await handler.Handle(
            new ConfirmOrderCommand { Id = order.Id, Note = "  All good  " },
            CancellationToken.None);

        response.Status.Should().Be(200);
        response.Data!.Status.Should().Be(nameof(OrderStatus.Confirmed));
        response.Data.StatusHistory.Should().HaveCount(1);
        response.Data.StatusHistory.Single().ToStatus.Should().Be(nameof(OrderStatus.Confirmed));
        response.Data.StatusHistory.Single().Note.Should().Be("All good");

        await using var assertionContext = CreateFreshContext();
        var updatedOrder = await assertionContext.Orders.Include(x => x.StatusHistory).FirstAsync(x => x.Id == order.Id);
        updatedOrder.Status.Should().Be(OrderStatus.Confirmed);
        updatedOrder.StatusHistory.Should().HaveCount(1);

        await inventoryModule.Received(1).ReserveStockAsync(
            Arg.Is<ReserveStockRequest>(x =>
                x.OrderId == order.Id &&
                x.Items.Count == 1 &&
                x.Items[0].VariantId == variantId &&
                x.Items[0].Quantity == 2),
            CancellationToken.None);

        await _mediator.Received(1).Publish(
            Arg.Is<OrderStatusChanged>(x => x.OrderId == order.Id && x.FromStatus == OrderStatus.Pending && x.ToStatus == OrderStatus.Confirmed),
            CancellationToken.None);
    }

    [Fact]
    public async Task Handle_InsufficientStock_ThrowsConflictAndLeavesOrderPending()
    {
        var variantId = Guid.NewGuid();
        var order = await SeedOrderAsync(OrderStatus.Pending, OrderTestFixture.CreateOrderItem(variantId, 2));
        var inventoryModule = OrderTestFixture.CreateInventoryModule();
        inventoryModule.ReserveStockAsync(Arg.Any<ReserveStockRequest>(), Arg.Any<CancellationToken>())
            .Returns<Task<InventoryAvailabilityResponse?>>(_ => throw new ConflictAppException("Insufficient available stock."));

        var handler = CreateHandler(_dbContext, inventoryModule);

        var act = () => handler.Handle(
            new ConfirmOrderCommand { Id = order.Id },
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictAppException>();

        await using var assertionContext = CreateFreshContext();
        var updatedOrder = await assertionContext.Orders.Include(x => x.StatusHistory).FirstAsync(x => x.Id == order.Id);
        updatedOrder.Status.Should().Be(OrderStatus.Pending);
        updatedOrder.StatusHistory.Should().BeEmpty();
        await _mediator.DidNotReceive().Publish(Arg.Any<OrderStatusChanged>(), Arg.Any<CancellationToken>());
    }
}
