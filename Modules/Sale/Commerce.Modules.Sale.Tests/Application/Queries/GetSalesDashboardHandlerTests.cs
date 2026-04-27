using Commerce.Modules.Sale.Application.Queries.GetSalesDashboard;
using Commerce.Modules.Sale.Domain;
using Commerce.Modules.Sale.Infrastructure;
using CommerceCore.Application.Abstractions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Commerce.Modules.Sale.Tests.Application.Queries;

public sealed class GetSalesDashboardHandlerTests
{
    private readonly SaleDbContext _context;
    private readonly GetSalesDashboardHandler _handler;

    public GetSalesDashboardHandlerTests()
    {
        var options = new DbContextOptionsBuilder<SaleDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dateTimeProvider = new Mock<IDateTimeProvider>();
        dateTimeProvider.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);

        var currentUser = new Mock<ICurrentUser>();
        currentUser.Setup(x => x.UserId).Returns("test-user");

        _context = new SaleDbContext(options, dateTimeProvider.Object, currentUser.Object);
        _handler = new GetSalesDashboardHandler(_context);
    }

    [Fact]
    public async Task Handle_WithVariousSales_ReturnsCorrectMetrics()
    {
        // Arrange
        var sales = new[]
        {
            new Domain.Sale
            {
                Id = Guid.NewGuid(),
                SaleNumber = "SALE-001",
                CustomerEmail = "test1@example.com",
                Status = SaleStatus.Draft,
                Currency = "USD",
                TotalAmount = 100m,
                CreatedAtUtc = DateTime.UtcNow.AddHours(-5)
            },
            new Domain.Sale
            {
                Id = Guid.NewGuid(),
                SaleNumber = "SALE-002",
                CustomerEmail = "test2@example.com",
                Status = SaleStatus.Priced,
                Currency = "USD",
                TotalAmount = 200m,
                CreatedAtUtc = DateTime.UtcNow.AddHours(-4)
            },
            new Domain.Sale
            {
                Id = Guid.NewGuid(),
                SaleNumber = "SALE-003",
                CustomerEmail = "test3@example.com",
                Status = SaleStatus.AwaitingPayment,
                Currency = "USD",
                TotalAmount = 150m,
                CreatedAtUtc = DateTime.UtcNow.AddHours(-3)
            },
            new Domain.Sale
            {
                Id = Guid.NewGuid(),
                SaleNumber = "SALE-004",
                CustomerEmail = "test4@example.com",
                Status = SaleStatus.Paid,
                Currency = "USD",
                TotalAmount = 300m,
                PaidAmount = 300m,
                CreatedAtUtc = DateTime.UtcNow.AddHours(-2)
            },
            new Domain.Sale
            {
                Id = Guid.NewGuid(),
                SaleNumber = "SALE-005",
                CustomerEmail = "test5@example.com",
                Status = SaleStatus.Paid,
                Currency = "USD",
                TotalAmount = 500m,
                PaidAmount = 500m,
                CreatedAtUtc = DateTime.UtcNow.AddHours(-1)
            },
            new Domain.Sale
            {
                Id = Guid.NewGuid(),
                SaleNumber = "SALE-006",
                CustomerEmail = "test6@example.com",
                Status = SaleStatus.Submitted,
                Currency = "USD",
                TotalAmount = 250m,
                CreatedAtUtc = DateTime.UtcNow.AddMinutes(-30)
            },
            new Domain.Sale
            {
                Id = Guid.NewGuid(),
                SaleNumber = "SALE-007",
                CustomerEmail = "test7@example.com",
                Status = SaleStatus.Expired,
                Currency = "USD",
                TotalAmount = 75m,
                CreatedAtUtc = DateTime.UtcNow.AddMinutes(-15)
            }
        };

        _context.Sales.AddRange(sales);
        await _context.SaveChangesAsync();

        var query = new GetSalesDashboardQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalDraftSales.Should().Be(1);
        result.TotalPricedSales.Should().Be(1);
        result.TotalAwaitingPayment.Should().Be(1);
        result.TotalPaidSales.Should().Be(2);
        result.TotalSubmittedSales.Should().Be(1);
        result.TotalExpiredSales.Should().Be(1);
        result.TotalRevenue.Should().Be(800m); // 300 + 500
        result.AverageOrderValue.Should().Be(400m); // 800 / 2
        result.RecentSales.Should().HaveCountLessOrEqualTo(10);
        result.RecentSales.Should().BeInDescendingOrder(s => s.CreatedAtUtc);
    }

    [Fact]
    public async Task Handle_NoSales_ReturnsZeroMetrics()
    {
        // Arrange
        var query = new GetSalesDashboardQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalDraftSales.Should().Be(0);
        result.TotalPricedSales.Should().Be(0);
        result.TotalAwaitingPayment.Should().Be(0);
        result.TotalPaidSales.Should().Be(0);
        result.TotalSubmittedSales.Should().Be(0);
        result.TotalExpiredSales.Should().Be(0);
        result.TotalRevenue.Should().Be(0);
        result.AverageOrderValue.Should().Be(0);
        result.RecentSales.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ExcludesDeletedSales()
    {
        // Arrange
        var sales = new[]
        {
            new Domain.Sale
            {
                Id = Guid.NewGuid(),
                SaleNumber = "SALE-001",
                CustomerEmail = "test1@example.com",
                Status = SaleStatus.Paid,
                Currency = "USD",
                TotalAmount = 100m,
                PaidAmount = 100m,
                CreatedAtUtc = DateTime.UtcNow,
                IsDeleted = false
            },
            new Domain.Sale
            {
                Id = Guid.NewGuid(),
                SaleNumber = "SALE-002",
                CustomerEmail = "test2@example.com",
                Status = SaleStatus.Paid,
                Currency = "USD",
                TotalAmount = 200m,
                PaidAmount = 200m,
                CreatedAtUtc = DateTime.UtcNow,
                IsDeleted = true
            }
        };

        _context.Sales.AddRange(sales);
        await _context.SaveChangesAsync();

        var query = new GetSalesDashboardQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalPaidSales.Should().Be(1);
        result.TotalRevenue.Should().Be(100m);
    }
}
