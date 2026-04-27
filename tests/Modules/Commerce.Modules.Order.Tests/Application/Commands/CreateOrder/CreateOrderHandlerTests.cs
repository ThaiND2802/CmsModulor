using Commerce.Modules.Order.Application.Commands.CreateOrder;
using Commerce.Modules.Order.Application.Events;
using Commerce.Modules.Order.Domain;
using Commerce.Modules.Order.Infrastructure;
using Commerce.Modules.Order.Tests.TestCommon;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Commerce.Modules.Order.Tests.Application.Commands.CreateOrder;

public sealed class CreateOrderHandlerTests : IDisposable
{
    private readonly OrderDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly CreateOrderHandler _handler;

    public CreateOrderHandlerTests()
    {
        _dbContext = OrderTestFixture.CreateInMemoryDbContext();
        _mediator = Substitute.For<IMediator>();
        _handler = new CreateOrderHandler(_dbContext, OrderTestFixture.CreateMapper(), _mediator);
    }

    private static CreateOrderCommand CreateValidCommand() =>
        OrderTestFixture.CreateValidCreateOrderCommand();

    public void Dispose() => _dbContext.Dispose();

    [Fact]
    public async Task Handle_ValidCommand_CreatesOrderWithCorrectTotals()
    {
        var command = CreateValidCommand();

        var response = await _handler.Handle(command, CancellationToken.None);

        response.Status.Should().Be(201);
        response.Data.Should().NotBeNull();

        var order = await _dbContext.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == response.Data!.Id);
        order.Should().NotBeNull();
        order!.SubtotalAmount.Should().Be(190m);
        order.TotalAmount.Should().Be(200m);
        order.OrderNumber.Should().NotBeNullOrEmpty();
        order.Currency.Should().Be("USD");
        order.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ValidCommand_AppendsPendingStatusHistory()
    {
        var response = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        var order = await _dbContext.Orders.Include(o => o.StatusHistory).FirstOrDefaultAsync(o => o.Id == response.Data!.Id);

        order!.StatusHistory.Should().HaveCount(1);
        var history = order.StatusHistory.First();
        history.ToStatus.Should().Be(OrderStatus.Pending);
        history.FromStatus.Should().BeNull();
    }

    [Fact]
    public async Task Handle_MultipleItems_SumsCorrectly()
    {
        var command = CreateValidCommand() with
        {
            Items =
            [
                new CreateOrderItemRequest(Guid.NewGuid(), "P1", "SKU1", null, null, 100m, 2, 0m),
                new CreateOrderItemRequest(Guid.NewGuid(), "P2", "SKU2", null, null, 50m, 3, 10m)
            ],
            DiscountAmount = 20m,
            ShippingAmount = 0m,
            TaxAmount = 0m
        };

        var response = await _handler.Handle(command, CancellationToken.None);

        var order = await _dbContext.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == response.Data!.Id);
        order!.SubtotalAmount.Should().Be(340m);
        order.TotalAmount.Should().Be(320m);
        order.Items.Should().HaveCount(2);
        order.Items.Sum(static item => item.TotalAmount).Should().Be(340m);
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesOrderCreatedEvent()
    {
        var response = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        await _mediator.Received(1).Publish(
            Arg.Is<OrderCreated>(x => x.OrderId == response.Data!.Id && x.OrderNumber == response.Data.OrderNumber),
            CancellationToken.None);
    }
}
