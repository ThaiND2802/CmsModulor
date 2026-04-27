using Commerce.Modules.Order.Application.Queries.GetMyOrders;
using Commerce.Modules.Order.Domain;
using Commerce.Modules.Order.Infrastructure;
using Commerce.Modules.Order.Tests.TestCommon;
using CommerceCore.Application.Abstractions;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Commerce.Modules.Order.Tests.Application.Queries.GetMyOrders;

public sealed class GetMyOrdersHandlerTests : IDisposable
{
    private readonly OrderDbContext _dbContext;
    private readonly GetMyOrdersHandler _handler;
    private readonly Guid _currentUserId;

    public GetMyOrdersHandlerTests()
    {
        _currentUserId = Guid.NewGuid();
        var currentUser = OrderTestFixture.CreateCurrentUser(_currentUserId.ToString());
        _dbContext = OrderTestFixture.CreateInMemoryDbContext(currentUser: currentUser);
        _handler = new GetMyOrdersHandler(_dbContext, OrderTestFixture.CreateMapper(), currentUser);
    }

    public void Dispose() => _dbContext.Dispose();

    [Fact]
    public async Task Handle_UnauthenticatedUser_ThrowsUnauthorizedException()
    {
        using var dbContext = OrderTestFixture.CreateInMemoryDbContext(currentUser: OrderTestFixture.CreateCurrentUser());
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.IsAuthenticated.Returns(false);
        var handler = new GetMyOrdersHandler(dbContext, OrderTestFixture.CreateMapper(), currentUser);

        var act = async () => await handler.Handle(new GetMyOrdersQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAppException>()
            .WithMessage("The current user is not authenticated.");
    }

    [Fact]
    public async Task Handle_InvalidUserId_ThrowsUnauthorizedException()
    {
        using var dbContext = OrderTestFixture.CreateInMemoryDbContext(currentUser: OrderTestFixture.CreateCurrentUser());
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.IsAuthenticated.Returns(true);
        currentUser.UserId.Returns("not-a-guid");
        var handler = new GetMyOrdersHandler(dbContext, OrderTestFixture.CreateMapper(), currentUser);

        var act = async () => await handler.Handle(new GetMyOrdersQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAppException>()
            .WithMessage("The current user is not authenticated.");
    }

    [Fact]
    public async Task Handle_ReturnsOnlyCurrentUsersOrders()
    {
        await SeedOrderAsync(_currentUserId, "ORD-MY-1");
        await SeedOrderAsync(_currentUserId, "ORD-MY-2");
        await SeedOrderAsync(Guid.NewGuid(), "ORD-OTHER");
        _dbContext.ChangeTracker.Clear();

        var response = await _handler.Handle(new GetMyOrdersQuery(), CancellationToken.None);

        response.Data.Should().HaveCount(2);
        response.Data.Select(static x => x.OrderNumber).Should().BeEquivalentTo("ORD-MY-1", "ORD-MY-2");
    }

    [Fact]
    public async Task Handle_PageZero_ThrowsValidationException()
    {
        var act = async () => await _handler.Handle(new GetMyOrdersQuery { Page = 0 }, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationAppException>()
            .WithMessage("Page must be greater than 0.");
    }

    private async Task SeedOrderAsync(Guid customerId, string orderNumber)
    {
        await _dbContext.Orders.AddAsync(OrderTestFixture.CreateOrderEntity(OrderStatus.Pending, customerId, orderNumber));
        await _dbContext.SaveChangesAsync();
    }
}
