using Commerce.Modules.Order.Application.Commands.ChangeOrderStatus;
using Commerce.Modules.Order.Application.Commands.CreateOrder;
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

namespace Commerce.Modules.Order.Tests.Application.Commands.ChangeOrderStatus;

public sealed class ChangeOrderStatusHandlerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly OrderDbContext _dbContext;
    private readonly IMediator _mediator;

    public ChangeOrderStatusHandlerTests()
    {
        _connection = OrderTestFixture.CreateSharedInMemoryConnection();
        _dbContext = OrderTestFixture.CreateInMemoryDbContext(_connection, ensureCreated: true);
        _mediator = Substitute.For<IMediator>();
    }

    private OrderDbContext CreateFreshContext() =>
        OrderTestFixture.CreateInMemoryDbContext(_connection);

    private ChangeOrderStatusHandler CreateHandler(OrderDbContext dbContext) =>
        new(dbContext, OrderTestFixture.CreateMapper(), _mediator);


    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }

    private async Task<OrderEntity> SeedOrderAsync(OrderStatus status)
    {
        var order = OrderTestFixture.CreateOrderEntity(status);
        await _dbContext.Orders.AddAsync(order);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();
        return order;
    }


    [Fact]
    public async Task Handle_PendingToConfirmed_ThrowsBusinessRuleException()
    {
        var createHandler = new CreateOrderHandler(_dbContext, OrderTestFixture.CreateMapper(), _mediator);
        var createResult = await createHandler.Handle(OrderTestFixture.CreateValidCreateOrderCommand(), CancellationToken.None);
        _dbContext.ChangeTracker.Clear();

        var order = createResult.Data!;
        var handler = CreateHandler(_dbContext);

        var act = async () => await handler.Handle(
            new ChangeOrderStatusCommand
            {
                Id = order.Id,
                ToStatus = OrderStatus.Confirmed,
                Note = "Confirmed payment"
            },
            CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleAppException>()
            .WithMessage("Cannot transition order from 'Pending' to 'Confirmed'.");

        await using var assertionContext = CreateFreshContext();
        var updatedOrder = await assertionContext.Orders.Include(o => o.StatusHistory).FirstAsync(o => o.Id == order.Id);
        updatedOrder.Status.Should().Be(OrderStatus.Pending);
        updatedOrder.StatusHistory.Should().HaveCount(1);

        await _mediator.DidNotReceive().Publish(
            Arg.Any<OrderStatusChanged>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConfirmedToProcessing_Succeeds()
    {
        var order = await SeedOrderAsync(OrderStatus.Confirmed);
        var handler = CreateHandler(_dbContext);

        var response = await handler.Handle(
            new ChangeOrderStatusCommand
            {
                Id = order.Id,
                ToStatus = OrderStatus.Processing,
                Note = "Start fulfillment"
            },
            CancellationToken.None);

        response.Status.Should().Be(200);

        await using var assertionContext = CreateFreshContext();
        var updatedOrder = await assertionContext.Orders.Include(o => o.StatusHistory).FirstAsync(o => o.Id == order.Id);
        updatedOrder.Status.Should().Be(OrderStatus.Processing);
        updatedOrder.StatusHistory.Should().HaveCount(1);

        var history = updatedOrder.StatusHistory.Single();
        history.FromStatus.Should().Be(OrderStatus.Confirmed);
        history.ToStatus.Should().Be(OrderStatus.Processing);
        history.Note.Should().Be("Start fulfillment");
        response.Data!.Status.Should().Be(nameof(OrderStatus.Processing));
        response.Data.StatusHistory.Should().HaveCount(1);

        await _mediator.Received(1).Publish(
            Arg.Is<OrderStatusChanged>(x => x.OrderId == order.Id && x.FromStatus == OrderStatus.Confirmed && x.ToStatus == OrderStatus.Processing),
            CancellationToken.None);
    }

    [Fact]
    public async Task Handle_DeliveredToPending_ThrowsBusinessRuleException()
    {
        var order = await SeedOrderAsync(OrderStatus.Delivered);
        var handler = CreateHandler(_dbContext);

        var act = async () => await handler.Handle(
            new ChangeOrderStatusCommand { Id = order.Id, ToStatus = OrderStatus.Pending },
            CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleAppException>()
            .WithMessage("Cannot transition order from 'Delivered' to 'Pending'.");
    }

    [Fact]
    public async Task Handle_ShippedToCancelled_ThrowsBusinessRuleException()
    {
        var order = await SeedOrderAsync(OrderStatus.Shipped);
        var handler = CreateHandler(_dbContext);

        var act = async () => await handler.Handle(
            new ChangeOrderStatusCommand { Id = order.Id, ToStatus = OrderStatus.Cancelled },
            CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleAppException>()
            .WithMessage("Cannot transition order from 'Shipped' to 'Cancelled'.");
    }
}
