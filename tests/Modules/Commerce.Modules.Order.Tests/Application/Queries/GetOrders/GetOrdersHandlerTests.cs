using Commerce.Modules.Order.Application.Queries.GetOrders;
using Commerce.Modules.Order.Domain;
using Commerce.Modules.Order.Infrastructure;
using Commerce.Modules.Order.Tests.TestCommon;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OrderEntity = Commerce.Modules.Order.Domain.Order;

namespace Commerce.Modules.Order.Tests.Application.Queries.GetOrders;

public sealed class GetOrdersHandlerTests : IDisposable
{
    private readonly MutableDateTimeProvider _dateTimeProvider;
    private readonly OrderDbContext _dbContext;
    private readonly GetOrdersHandler _handler;

    public GetOrdersHandlerTests()
    {
        _dateTimeProvider = OrderTestFixture.CreateMutableDateTimeProvider(new DateTime(2026, 4, 25, 12, 0, 0, DateTimeKind.Utc));
        _dbContext = OrderTestFixture.CreateInMemoryDbContext(_dateTimeProvider);
        _handler = new GetOrdersHandler(_dbContext, OrderTestFixture.CreateMapper());
    }

    public void Dispose() => _dbContext.Dispose();

    private async Task<Guid> SeedOrdersAsync()
    {
        var customerId = Guid.NewGuid();
        var first = OrderTestFixture.CreateOrderEntity(OrderStatus.Pending, customerId, "ORD-001", "alpha@test.com", totalAmount: 100m);
        first.Items.Add(new OrderItem { Id = Guid.NewGuid(), OrderId = first.Id, ProductId = Guid.NewGuid(), ProductName = "P1", ProductSku = "SKU1", UnitPrice = 100m, Quantity = 1, DiscountAmount = 0m, TotalAmount = 100m });
        await _dbContext.Orders.AddAsync(first);
        await _dbContext.SaveChangesAsync();

        OrderTestFixture.EnsureIncreasingTime(_dateTimeProvider);

        var second = OrderTestFixture.CreateOrderEntity(OrderStatus.Confirmed, customerId, "ORD-002", "beta@test.com", totalAmount: 200m);
        second.Items.Add(new OrderItem { Id = Guid.NewGuid(), OrderId = second.Id, ProductId = Guid.NewGuid(), ProductName = "P2", ProductSku = "SKU2", UnitPrice = 100m, Quantity = 2, DiscountAmount = 0m, TotalAmount = 200m });
        await _dbContext.Orders.AddAsync(second);
        await _dbContext.SaveChangesAsync();

        OrderTestFixture.EnsureIncreasingTime(_dateTimeProvider);

        var third = OrderTestFixture.CreateOrderEntity(OrderStatus.Shipped, Guid.NewGuid(), "ORD-003", "gamma@test.com", totalAmount: 300m);
        third.Items.Add(new OrderItem { Id = Guid.NewGuid(), OrderId = third.Id, ProductId = Guid.NewGuid(), ProductName = "P3", ProductSku = "SKU3", UnitPrice = 300m, Quantity = 1, DiscountAmount = 0m, TotalAmount = 300m });
        await _dbContext.Orders.AddAsync(third);
        await _dbContext.SaveChangesAsync();

        return customerId;
    }

    private async Task AssertValidationExceptionAsync(GetOrdersQuery query, string expectedMessage)
    {
        var act = async () => await _handler.Handle(query, CancellationToken.None);
        await act.Should().ThrowAsync<ValidationAppException>()
            .WithMessage(expectedMessage);
    }

    [Fact]
    public async Task Handle_PageZero_ThrowsValidationException()
    {
        await AssertValidationExceptionAsync(new GetOrdersQuery { Page = 0 }, "Page must be greater than 0.");
    }

    [Fact]
    public async Task Handle_PageSizeZero_ThrowsValidationException()
    {
        await AssertValidationExceptionAsync(new GetOrdersQuery { PageSize = 0 }, "Page size must be greater than 0.");
    }

    [Fact]
    public async Task Handle_Search_ReturnsMatchingOrders()
    {
        await SeedOrdersAsync();

        var response = await _handler.Handle(new GetOrdersQuery { Search = "beta" }, CancellationToken.None);

        response.Data.Should().ContainSingle();
        response.Data.Single().OrderNumber.Should().Be("ORD-002");
    }

    [Fact]
    public async Task Handle_FilterByDateRange_ReturnsMatchingOrders()
    {
        await SeedOrdersAsync();
        var threshold = await _dbContext.Orders.Where(static order => order.OrderNumber == "ORD-001").Select(static order => order.CreatedAtUtc).FirstAsync();

        var response = await _handler.Handle(
            new GetOrdersQuery { FromDate = threshold.AddMilliseconds(1) },
            CancellationToken.None);

        response.Data.Should().HaveCount(2);
        response.Data.Select(static order => order.OrderNumber).Should().BeEquivalentTo("ORD-002", "ORD-003");
    }

    [Fact]
    public async Task Handle_DefaultSort_ReturnsAllOrders()
    {
        await SeedOrdersAsync();

        var response = await _handler.Handle(new GetOrdersQuery(), CancellationToken.None);

        response.Data.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_SortByTotalAmountAscending_ReturnsSortedOrders()
    {
        await SeedOrdersAsync();

        var response = await _handler.Handle(
            new GetOrdersQuery { SortBy = "totalAmount", SortDescending = false },
            CancellationToken.None);

        response.Data.Select(static order => order.TotalAmount).Should().Equal(100m, 200m, 300m);
    }

    [Fact]
    public async Task Handle_ExcludesSoftDeletedOrders()
    {
        await SeedOrdersAsync();

        var deletedOrder = OrderTestFixture.CreateOrderEntity(
            orderNumber: "ORD-004",
            customerEmail: "deleted@test.com",
            isDeleted: true);

        await _dbContext.Orders.AddAsync(deletedOrder);
        await _dbContext.SaveChangesAsync();

        var response = await _handler.Handle(new GetOrdersQuery(), CancellationToken.None);

        response.Data.Select(static order => order.OrderNumber).Should().NotContain("ORD-004");
    }

    [Fact]
    public async Task Handle_MapsItemCount_FromIncludedItems()
    {
        await SeedOrdersAsync();

        var response = await _handler.Handle(new GetOrdersQuery { Search = "ORD-002" }, CancellationToken.None);

        response.Data.Single().ItemCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_FilterByCustomerId_ReturnsMatchingOrders()
    {
        var customerId = await SeedOrdersAsync();

        var response = await _handler.Handle(new GetOrdersQuery { CustomerId = customerId }, CancellationToken.None);

        response.Data.Should().HaveCount(2);
        response.Data.Select(static o => o.OrderNumber).Should().BeEquivalentTo("ORD-001", "ORD-002");
    }

    [Fact]
    public async Task Handle_FilterByStatus_ReturnsMatchingOrders()
    {
        await SeedOrdersAsync();

        var response = await _handler.Handle(new GetOrdersQuery { Status = OrderStatus.Pending }, CancellationToken.None);

        response.Data.Should().HaveCount(1);
        response.Data.First().OrderNumber.Should().Be("ORD-001");
    }

    [Fact]
    public async Task Handle_Pagination_ReturnsCorrectPage()
    {
        await SeedOrdersAsync();

        var response = await _handler.Handle(
            new GetOrdersQuery { Page = 2, PageSize = 2, SortBy = "totalAmount", SortDescending = false },
            CancellationToken.None);

        response.Data.Should().HaveCount(1);
        response.Data.First().OrderNumber.Should().Be("ORD-003");
        response.Pagination.Total.Should().Be(3);
        response.Pagination.Page.Should().Be(2);
        response.Pagination.PageSize.Should().Be(2);
    }
}
