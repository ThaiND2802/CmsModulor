using Commerce.Modules.Sale.Application.Commands.SelectPaymentMethod;
using Commerce.Modules.Sale.Domain;
using Commerce.Modules.Sale.Infrastructure;
using CommerceCore.Application.Abstractions;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Commerce.Modules.Sale.Tests.Application.Commands;

public sealed class SelectPaymentMethodHandlerTests
{
    private readonly SaleDbContext _context;
    private readonly SelectPaymentMethodHandler _handler;

    public SelectPaymentMethodHandlerTests()
    {
        var options = new DbContextOptionsBuilder<SaleDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var dateTimeProvider = new Mock<IDateTimeProvider>();
        dateTimeProvider.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);

        var currentUser = new Mock<ICurrentUser>();
        currentUser.Setup(x => x.UserId).Returns("test-user");

        _context = new SaleDbContext(options, dateTimeProvider.Object, currentUser.Object);
        _handler = new SelectPaymentMethodHandler(_context);
    }

    [Fact]
    public async Task Handle_ValidPricedSale_SelectsPaymentMethod()
    {
        // Arrange
        var sale = new Domain.Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = "SALE-001",
            CustomerEmail = "test@example.com",
            Status = SaleStatus.Priced,
            Currency = "USD",
            TotalAmount = 100m,
            CreatedAtUtc = DateTime.UtcNow
        };
        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();

        var command = new SelectPaymentMethodCommand(sale.Id, PaymentMethod.CreditCard);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var updated = await _context.Sales.FindAsync(sale.Id);
        updated.Should().NotBeNull();
        updated!.PaymentMethod.Should().Be(PaymentMethod.CreditCard);
    }

    [Fact]
    public async Task Handle_InvalidStatus_ThrowsBusinessRuleException()
    {
        // Arrange
        var sale = new Domain.Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = "SALE-003",
            CustomerEmail = "test@example.com",
            Status = SaleStatus.Draft,
            Currency = "USD",
            TotalAmount = 100m,
            CreatedAtUtc = DateTime.UtcNow
        };
        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();

        var command = new SelectPaymentMethodCommand(sale.Id, PaymentMethod.Cash);

        // Act & Assert
        await _handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<BusinessRuleAppException>();
    }

    [Fact]
    public async Task Handle_NonExistentSale_ThrowsNotFoundException()
    {
        // Arrange
        var command = new SelectPaymentMethodCommand(Guid.NewGuid(), PaymentMethod.Cash);

        // Act & Assert
        await _handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<NotFoundAppException>();
    }
}
