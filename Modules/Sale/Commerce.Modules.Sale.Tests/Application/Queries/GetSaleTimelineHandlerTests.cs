using Commerce.Modules.Sale.Application.Queries.GetSaleTimeline;
using Commerce.Modules.Sale.Domain;
using Commerce.Modules.Sale.Infrastructure;
using CommerceCore.Application.Abstractions;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Commerce.Modules.Sale.Tests.Application.Queries;

public sealed class GetSaleTimelineHandlerTests
{
    private readonly SaleDbContext _context;
    private readonly GetSaleTimelineHandler _handler;

    public GetSaleTimelineHandlerTests()
    {
        var options = new DbContextOptionsBuilder<SaleDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dateTimeProvider = new Mock<IDateTimeProvider>();
        dateTimeProvider.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);

        var currentUser = new Mock<ICurrentUser>();
        currentUser.Setup(x => x.UserId).Returns("test-user");

        _context = new SaleDbContext(options, dateTimeProvider.Object, currentUser.Object);
        _handler = new GetSaleTimelineHandler(_context);
    }

    [Fact]
    public async Task Handle_SaleWithFullLifecycle_ReturnsCompleteTimeline()
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddHours(-2);
        var sale = new Domain.Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = "SALE-001",
            CustomerEmail = "test@example.com",
            Status = SaleStatus.Paid,
            PaymentMethod = PaymentMethod.CreditCard,
            PaymentStatus = PaymentStatus.Completed,
            PaidAmount = 100m,
            PaymentReference = "REF-12345",
            CouponCode = "SAVE10",
            CouponType = "Percentage",
            CouponDiscountAmount = 10m,
            Currency = "USD",
            TotalAmount = 90m,
            CreatedAtUtc = createdAt,
            CreatedBy = "user1",
            PaymentInitiatedAtUtc = createdAt.AddMinutes(30),
            PaidAtUtc = createdAt.AddMinutes(35),
            SubmittedAtUtc = createdAt.AddMinutes(40),
            OrderId = Guid.NewGuid()
        };

        sale.StatusHistory.Add(new SaleStatusHistory
        {
            Id = Guid.NewGuid(),
            SaleId = sale.Id,
            FromStatus = SaleStatus.Draft,
            ToStatus = SaleStatus.Priced,
            ChangedAtUtc = createdAt.AddMinutes(10),
            ChangedBy = "user1"
        });

        sale.StatusHistory.Add(new SaleStatusHistory
        {
            Id = Guid.NewGuid(),
            SaleId = sale.Id,
            FromStatus = SaleStatus.Priced,
            ToStatus = SaleStatus.AwaitingPayment,
            ChangedAtUtc = createdAt.AddMinutes(30),
            ChangedBy = "user1"
        });

        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();

        var query = new GetSaleTimelineQuery(sale.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.SaleId.Should().Be(sale.Id);
        result.SaleNumber.Should().Be("SALE-001");
        result.Events.Should().NotBeEmpty();
        result.Events.Should().Contain(e => e.EventType == "SaleCreated");
        result.Events.Should().Contain(e => e.EventType == "StatusChanged");
        result.Events.Should().Contain(e => e.EventType == "CouponApplied");
        result.Events.Should().Contain(e => e.EventType == "PaymentInitiated");
        result.Events.Should().Contain(e => e.EventType == "PaymentCompleted");
        result.Events.Should().Contain(e => e.EventType == "SaleSubmitted");
        result.Events.Should().BeInAscendingOrder(e => e.OccurredAtUtc);
    }

    [Fact]
    public async Task Handle_NonExistentSale_ThrowsNotFoundException()
    {
        // Arrange
        var query = new GetSaleTimelineQuery(Guid.NewGuid());

        // Act & Assert
        await _handler.Invoking(h => h.Handle(query, CancellationToken.None))
            .Should().ThrowAsync<NotFoundAppException>();
    }

    [Fact]
    public async Task Handle_MinimalSale_ReturnsBasicTimeline()
    {
        // Arrange
        var sale = new Domain.Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = "SALE-002",
            CustomerEmail = "test@example.com",
            Status = SaleStatus.Draft,
            Currency = "USD",
            TotalAmount = 50m,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = "user2"
        };
        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();

        var query = new GetSaleTimelineQuery(sale.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Events.Should().HaveCount(1);
        result.Events[0].EventType.Should().Be("SaleCreated");
    }
}
