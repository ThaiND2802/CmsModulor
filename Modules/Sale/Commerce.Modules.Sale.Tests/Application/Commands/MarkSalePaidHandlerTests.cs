using Commerce.Modules.Sale.Application.Commands.MarkSalePaid;
using Commerce.Modules.Sale.Domain;
using Commerce.Modules.Sale.Infrastructure;
using CommerceCore.Application.Abstractions;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Commerce.Modules.Sale.Tests.Application.Commands;

public sealed class MarkSalePaidHandlerTests
{
    private readonly SaleDbContext _context;
    private readonly MarkSalePaidHandler _handler;

    public MarkSalePaidHandlerTests()
    {
        var options = new DbContextOptionsBuilder<SaleDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dateTimeProvider = new Mock<IDateTimeProvider>();
        dateTimeProvider.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);

        var currentUser = new Mock<ICurrentUser>();
        currentUser.Setup(x => x.UserId).Returns("test-user");

        _context = new SaleDbContext(options, dateTimeProvider.Object, currentUser.Object);
        _handler = new MarkSalePaidHandler(_context);
    }

    [Fact]
    public async Task Handle_PricedSale_WithSelectedPaymentMethod_MarksPaid()
    {
        // Arrange
        var sale = new Domain.Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = "SALE-001",
            CustomerEmail = "test@example.com",
            Status = SaleStatus.Priced,
            PaymentMethod = PaymentMethod.CreditCard,
            PaymentStatus = PaymentStatus.Unpaid,
            Currency = "USD",
            TotalAmount = 100m,
            CreatedAtUtc = DateTime.UtcNow
        };
        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();

        var command = new MarkSalePaidCommand(sale.Id, 100m, "REF-12345");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await _context.Sales.FindAsync(sale.Id);
        updated.Should().NotBeNull();
        updated!.Status.Should().Be(SaleStatus.Priced);
        updated.PaymentStatus.Should().Be(PaymentStatus.Paid);
        updated.PaidAmount.Should().Be(100m);
        updated.PaymentReference.Should().Be("REF-12345");
        updated.PaidAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_PricedSale_MarksPaid()
    {
        // Arrange
        var sale = new Domain.Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = "SALE-002",
            CustomerEmail = "test@example.com",
            Status = SaleStatus.Priced,
            PaymentMethod = PaymentMethod.Cash,
            Currency = "USD",
            TotalAmount = 50m,
            CreatedAtUtc = DateTime.UtcNow
        };
        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();

        var command = new MarkSalePaidCommand(sale.Id, 50m);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await _context.Sales.FindAsync(sale.Id);
        updated.Should().NotBeNull();
        updated!.Status.Should().Be(SaleStatus.Priced);
        updated.PaymentStatus.Should().Be(PaymentStatus.Paid);
        updated.PaidAmount.Should().Be(50m);
    }

    [Fact]
    public async Task Handle_AlreadyPaidSale_IsIdempotent()
    {
        // Arrange
        var sale = new Domain.Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = "SALE-003",
            CustomerEmail = "test@example.com",
            Status = SaleStatus.Priced,
            PaymentStatus = PaymentStatus.Paid,
            PaymentMethod = PaymentMethod.CreditCard,
            PaidAmount = 100m,
            PaymentReference = "REF-ORIGINAL",
            Currency = "USD",
            TotalAmount = 100m,
            CreatedAtUtc = DateTime.UtcNow,
            PaidAtUtc = DateTime.UtcNow.AddMinutes(-5)
        };
        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();

        var originalPaidAt = sale.PaidAtUtc;
        var command = new MarkSalePaidCommand(sale.Id, 100m, "REF-DUPLICATE");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await _context.Sales.FindAsync(sale.Id);
        updated.Should().NotBeNull();
        updated!.Status.Should().Be(SaleStatus.Priced);
        updated.PaymentStatus.Should().Be(PaymentStatus.Paid);
        updated.PaidAmount.Should().Be(100m);
        updated.PaymentReference.Should().Be("REF-ORIGINAL"); // Should not change
        updated.PaidAtUtc.Should().Be(originalPaidAt); // Should not change
    }

    [Fact]
    public async Task Handle_InvalidStatus_ThrowsBusinessRuleException()
    {
        // Arrange
        var sale = new Domain.Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = "SALE-004",
            CustomerEmail = "test@example.com",
            Status = SaleStatus.Draft,
            Currency = "USD",
            TotalAmount = 100m,
            CreatedAtUtc = DateTime.UtcNow
        };
        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();

        var command = new MarkSalePaidCommand(sale.Id, 100m);

        // Act & Assert
        await _handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<BusinessRuleAppException>();
    }

    [Fact]
    public async Task Handle_NonExistentSale_ThrowsNotFoundException()
    {
        // Arrange
        var command = new MarkSalePaidCommand(Guid.NewGuid(), 100m);

        // Act & Assert
        await _handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<NotFoundAppException>();
    }
}
