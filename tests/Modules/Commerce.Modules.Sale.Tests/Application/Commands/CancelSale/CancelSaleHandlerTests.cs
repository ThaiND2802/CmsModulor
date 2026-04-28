using Commerce.Modules.Inventory.Contracts.Requests;
using Commerce.Modules.Sale.Application.Commands.CancelSale;
using Commerce.Modules.Sale.Application.Services;
using Commerce.Modules.Sale.Domain;
using Commerce.Modules.Sale.Tests.TestCommon;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Commerce.Modules.Sale.Tests.Application.Commands.CancelSale;

public sealed class CancelSaleHandlerTests
{
    [Fact]
    public async Task Handle_CancelsPricedSale()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        await using var seedContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var sale = SaleTestFixture.CreatePricedSaleEntity();
        seedContext.Sales.Add(sale);
        await seedContext.SaveChangesAsync();

        await using var dbContext = SaleTestFixture.CreateSaleDbContext(databaseName);
        var inventoryModule = SaleTestFixture.CreateInventoryModule();
        var handler = new CancelSaleHandler(
            new SaleSubmissionService(dbContext, inventoryModule),
            SaleTestFixture.CreateMapper());

        var response = await handler.Handle(
            new CancelSaleCommand { SaleId = sale.Id, Reason = "Operator cancelled sale." },
            CancellationToken.None);

        var updatedSale = await dbContext.Sales.Include(x => x.StatusHistory).SingleAsync(x => x.Id == sale.Id);
        response.Status.Should().Be(200);
        updatedSale.Status.Should().Be(SaleStatus.Cancelled);
        updatedSale.OrderId.Should().BeNull();
        updatedSale.PaymentStatus.Should().Be(PaymentStatus.Unpaid);
        updatedSale.PaidAmount.Should().Be(0m);
        updatedSale.PaymentReference.Should().BeNull();
        updatedSale.PaidAtUtc.Should().BeNull();
        updatedSale.StatusHistory.Should().Contain(x => x.ToStatus == SaleStatus.Cancelled);
        updatedSale.StatusHistory.Should().NotContain(x => x.ToStatus == SaleStatus.Submitted);
        await inventoryModule.DidNotReceive().ReserveStockAsync(Arg.Any<ReserveStockRequest>(), Arg.Any<CancellationToken>());
        await inventoryModule.DidNotReceive().ReleaseStockAsync(Arg.Any<ReleaseStockRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_RejectsSubmittedSale()
    {
        await using var dbContext = SaleTestFixture.CreateSaleDbContext();
        var orderId = Guid.NewGuid();
        var paidAtUtc = new DateTime(2026, 4, 27, 0, 0, 0, DateTimeKind.Utc);
        var sale = SaleTestFixture.CreateSaleEntity(SaleStatus.Submitted);
        sale.OrderId = orderId;
        sale.PaymentStatus = PaymentStatus.Paid;
        sale.PaidAmount = sale.TotalAmount;
        sale.PaymentReference = "PAY-TEST-001";
        sale.PaidAtUtc = paidAtUtc;
        sale.StatusHistory.Add(new SaleStatusHistory
        {
            Id = Guid.NewGuid(),
            SaleId = sale.Id,
            FromStatus = SaleStatus.Priced,
            ToStatus = SaleStatus.Submitted,
            ChangedAtUtc = paidAtUtc
        });
        dbContext.Sales.Add(sale);
        await dbContext.SaveChangesAsync();

        var inventoryModule = SaleTestFixture.CreateInventoryModule();
        var handler = new CancelSaleHandler(
            new SaleSubmissionService(dbContext, inventoryModule),
            SaleTestFixture.CreateMapper());

        var act = () => handler.Handle(new CancelSaleCommand { SaleId = sale.Id }, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleAppException>();

        var updatedSale = await dbContext.Sales.Include(x => x.StatusHistory).SingleAsync(x => x.Id == sale.Id);
        updatedSale.Status.Should().Be(SaleStatus.Submitted);
        updatedSale.OrderId.Should().Be(orderId);
        updatedSale.PaymentStatus.Should().Be(PaymentStatus.Paid);
        updatedSale.PaidAmount.Should().Be(sale.TotalAmount);
        updatedSale.PaymentReference.Should().Be("PAY-TEST-001");
        updatedSale.PaidAtUtc.Should().Be(paidAtUtc);
        updatedSale.StatusHistory.Should().NotContain(x => x.ToStatus == SaleStatus.Cancelled);
        await inventoryModule.DidNotReceive().ReserveStockAsync(Arg.Any<ReserveStockRequest>(), Arg.Any<CancellationToken>());
        await inventoryModule.DidNotReceive().ReleaseStockAsync(Arg.Any<ReleaseStockRequest>(), Arg.Any<CancellationToken>());
    }
}
