using Commerce.Modules.Inventory.Application.Commands.ReleaseStock;
using Commerce.Modules.Inventory.Domain;
using Commerce.Modules.Inventory.Tests.TestCommon;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Commerce.Modules.Inventory.Tests.Application.Commands.ReleaseStock;

public sealed class ReleaseStockHandlerTests : IDisposable
{
    private readonly string _databaseName;
    private readonly Commerce.Modules.Inventory.Infrastructure.InventoryDbContext _dbContext;
    private readonly IMediator _mediator;

    public ReleaseStockHandlerTests()
    {
        _databaseName = Guid.NewGuid().ToString();
        _dbContext = InventoryTestFixture.CreateInventoryDbContext(_databaseName);
        _mediator = Substitute.For<IMediator>();
    }

    public void Dispose() => _dbContext.Dispose();

    private Commerce.Modules.Inventory.Infrastructure.InventoryDbContext CreateFreshContext() =>
        InventoryTestFixture.CreateInventoryDbContext(_databaseName);

    private ReleaseStockHandler CreateHandler(Commerce.Modules.Inventory.Infrastructure.InventoryDbContext dbContext) =>
        new(dbContext, InventoryTestFixture.CreateMapper(), _mediator);

    [Fact]
    public async Task Handle_ActiveReservation_ReleasesStock()
    {
        var variantId = Guid.NewGuid();
        var item = InventoryTestFixture.CreateInventoryItem(variantId, onHand: 10, reserved: 4);
        var reservation = StockReservation.Create(Guid.NewGuid());
        reservation.Items.Add(StockReservationItem.Create(reservation.Id, item.Id, 4));

        await _dbContext.InventoryItems.AddAsync(item);
        await _dbContext.StockReservations.AddAsync(reservation);
        await _dbContext.SaveChangesAsync();

        var response = await CreateHandler(CreateFreshContext()).Handle(new ReleaseStockCommand(reservation.OrderId), CancellationToken.None);

        response.Status.Should().Be(200);
        response.Data.Should().NotBeNull();
        response.Data!.Status.Should().Be(nameof(StockReservationStatus.Released));

        await using var assertionContext = CreateFreshContext();
        var updatedItem = await assertionContext.InventoryItems.SingleAsync(x => x.Id == item.Id);
        updatedItem.ReservedQuantity.Should().Be(0);

        var updatedReservation = await assertionContext.StockReservations.SingleAsync(x => x.Id == reservation.Id);
        updatedReservation.Status.Should().Be(StockReservationStatus.Released);
        updatedReservation.ReleasedAtUtc.Should().NotBeNull();

        var movement = await assertionContext.StockMovements.SingleAsync(x => x.ReferenceId == reservation.OrderId);
        movement.MovementType.Should().Be(StockMovementType.Released);
        movement.QuantityDelta.Should().Be(-4);
    }

    [Fact]
    public async Task Handle_MultipleReservationItems_CreatesMovementPerMutation()
    {
        var firstVariantId = Guid.NewGuid();
        var secondVariantId = Guid.NewGuid();
        var firstItem = InventoryTestFixture.CreateInventoryItem(firstVariantId, onHand: 10, reserved: 4);
        var secondItem = InventoryTestFixture.CreateInventoryItem(secondVariantId, onHand: 8, reserved: 2);
        var reservation = StockReservation.Create(Guid.NewGuid());
        reservation.Items.Add(StockReservationItem.Create(reservation.Id, firstItem.Id, 4));
        reservation.Items.Add(StockReservationItem.Create(reservation.Id, secondItem.Id, 2));

        await _dbContext.InventoryItems.AddRangeAsync(firstItem, secondItem);
        await _dbContext.StockReservations.AddAsync(reservation);
        await _dbContext.SaveChangesAsync();

        await CreateHandler(CreateFreshContext()).Handle(new ReleaseStockCommand(reservation.OrderId), CancellationToken.None);

        await using var assertionContext = CreateFreshContext();
        var movements = await assertionContext.StockMovements
            .Where(x => x.ReferenceId == reservation.OrderId)
            .OrderBy(x => x.QuantityDelta)
            .ToListAsync();

        movements.Should().HaveCount(2);
        movements.Should().OnlyContain(x => x.MovementType == StockMovementType.Released && x.ReferenceType == "order");
    }

    [Fact]
    public async Task Handle_AlreadyReleasedReservation_IsIdempotent()
    {
        var variantId = Guid.NewGuid();
        var item = InventoryTestFixture.CreateInventoryItem(variantId, onHand: 10, reserved: 0);
        var reservation = StockReservation.Create(Guid.NewGuid());
        reservation.Items.Add(StockReservationItem.Create(reservation.Id, item.Id, 4));
        reservation.Release(DateTime.UtcNow);

        await _dbContext.InventoryItems.AddAsync(item);
        await _dbContext.StockReservations.AddAsync(reservation);
        await _dbContext.SaveChangesAsync();

        var response = await CreateHandler(CreateFreshContext()).Handle(new ReleaseStockCommand(reservation.OrderId), CancellationToken.None);

        response.Status.Should().Be(200);
        response.Data!.Status.Should().Be(nameof(StockReservationStatus.Released));

        await using var assertionContext = CreateFreshContext();
        (await assertionContext.StockMovements.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Handle_MissingReservation_ThrowsNotFound()
    {
        var act = () => CreateHandler(CreateFreshContext()).Handle(new ReleaseStockCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundAppException>()
            .WithMessage("*Reservation for order*");
    }
}
