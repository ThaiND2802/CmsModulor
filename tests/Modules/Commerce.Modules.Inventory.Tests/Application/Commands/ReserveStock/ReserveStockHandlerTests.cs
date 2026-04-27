using Commerce.Modules.Catalog.Contracts;
using Commerce.Modules.Inventory.Application.Commands.ReserveStock;
using Commerce.Modules.Inventory.Application.Events;
using Commerce.Modules.Inventory.Domain;
using Commerce.Modules.Inventory.Tests.TestCommon;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Commerce.Modules.Inventory.Tests.Application.Commands.ReserveStock;

public sealed class ReserveStockHandlerTests : IDisposable
{
    private readonly string _databaseName;
    private readonly Commerce.Modules.Inventory.Infrastructure.InventoryDbContext _dbContext;
    private readonly IMediator _mediator;

    public ReserveStockHandlerTests()
    {
        _databaseName = Guid.NewGuid().ToString();
        _dbContext = InventoryTestFixture.CreateInventoryDbContext(_databaseName);
        _mediator = Substitute.For<IMediator>();
    }

    public void Dispose() => _dbContext.Dispose();

    private Commerce.Modules.Inventory.Infrastructure.InventoryDbContext CreateFreshContext() =>
        InventoryTestFixture.CreateInventoryDbContext(_databaseName);

    private ReserveStockHandler CreateHandler(
        Commerce.Modules.Inventory.Infrastructure.InventoryDbContext dbContext,
        ICatalogVariantLookup? catalogVariantLookup = null) =>
        new(
            dbContext,
            catalogVariantLookup ?? InventoryTestFixture.CreateCatalogVariantLookup(),
            InventoryTestFixture.CreateMapper(),
            _mediator);

    private static ICatalogVariantLookup CreateLookupFor(params Guid[] variantIds) =>
        InventoryTestFixture.CreateCatalogVariantLookup(
            variantIds.Select(variantId => InventoryTestFixture.CreateCatalogVariantSummary(variantId)).ToArray());

    private static ICatalogVariantLookup CreateInactiveLookup(Guid variantId) =>
        InventoryTestFixture.CreateCatalogVariantLookup(
            InventoryTestFixture.CreateCatalogVariantSummary(variantId, isActive: false));

    [Fact]
    public async Task Handle_ValidCommand_CreatesReservationAndPublishesEvent()
    {
        var variantId = Guid.NewGuid();
        await _dbContext.InventoryItems.AddAsync(InventoryTestFixture.CreateInventoryItem(variantId, onHand: 10));
        await _dbContext.SaveChangesAsync();

        var command = new ReserveStockCommand
        {
            OrderId = Guid.NewGuid(),
            Items = [new ReserveStockItemRequest(variantId, 3)]
        };

        var response = await CreateHandler(CreateFreshContext(), CreateLookupFor(variantId)).Handle(command, CancellationToken.None);

        response.Status.Should().Be(201);
        response.Data.Should().NotBeNull();
        response.Data!.Items.Should().ContainSingle();

        await using var assertionContext = CreateFreshContext();
        var inventoryItem = await assertionContext.InventoryItems.SingleAsync(x => x.VariantId == variantId);
        inventoryItem.ReservedQuantity.Should().Be(3);

        var reservation = await assertionContext.StockReservations
            .Include(x => x.Items)
            .ThenInclude(x => x.InventoryItem)
            .SingleAsync(x => x.OrderId == command.OrderId);
        reservation.Status.Should().Be(StockReservationStatus.Active);
        reservation.Items.Should().ContainSingle(x => x.Quantity == 3);

        var movement = await assertionContext.StockMovements.SingleAsync(x => x.ReferenceId == command.OrderId);
        movement.MovementType.Should().Be(StockMovementType.Reserved);
        movement.QuantityDelta.Should().Be(3);

        await _mediator.Received(1).Publish(
            Arg.Is<StockReserved>(x => x.OrderId == command.OrderId && x.Items.Count == 1 && x.Items[0].Quantity == 3),
            CancellationToken.None);
    }

    [Fact]
    public async Task Handle_MultipleItems_CreatesMovementPerMutation()
    {
        var firstVariantId = Guid.NewGuid();
        var secondVariantId = Guid.NewGuid();
        await _dbContext.InventoryItems.AddRangeAsync(
            InventoryTestFixture.CreateInventoryItem(firstVariantId, onHand: 10),
            InventoryTestFixture.CreateInventoryItem(secondVariantId, onHand: 8));
        await _dbContext.SaveChangesAsync();

        var command = new ReserveStockCommand
        {
            OrderId = Guid.NewGuid(),
            Items =
            [
                new ReserveStockItemRequest(firstVariantId, 3),
                new ReserveStockItemRequest(secondVariantId, 2)
            ]
        };

        await CreateHandler(CreateFreshContext(), CreateLookupFor(firstVariantId, secondVariantId)).Handle(command, CancellationToken.None);

        await using var assertionContext = CreateFreshContext();
        var movements = await assertionContext.StockMovements
            .Where(x => x.ReferenceId == command.OrderId)
            .OrderBy(x => x.QuantityDelta)
            .ToListAsync();

        movements.Should().HaveCount(2);
        movements.Should().OnlyContain(x => x.MovementType == StockMovementType.Reserved && x.ReferenceType == "order");
    }

    [Fact]
    public async Task Handle_SameOrderAndItems_ReturnsExistingReservation()
    {
        var variantId = Guid.NewGuid();
        await _dbContext.InventoryItems.AddAsync(InventoryTestFixture.CreateInventoryItem(variantId, onHand: 10));
        await _dbContext.SaveChangesAsync();

        var command = new ReserveStockCommand
        {
            OrderId = Guid.NewGuid(),
            Items = [new ReserveStockItemRequest(variantId, 2)]
        };

        var handler = CreateHandler(CreateFreshContext(), CreateLookupFor(variantId));
        await handler.Handle(command, CancellationToken.None);
        _mediator.ClearReceivedCalls();

        var secondResponse = await CreateHandler(CreateFreshContext(), InventoryTestFixture.CreateCatalogVariantLookup()).Handle(command, CancellationToken.None);

        secondResponse.Status.Should().Be(200);

        await using var assertionContext = CreateFreshContext();
        var inventoryItem = await assertionContext.InventoryItems.SingleAsync(x => x.VariantId == variantId);
        inventoryItem.ReservedQuantity.Should().Be(2);
        (await assertionContext.StockReservations.CountAsync(x => x.OrderId == command.OrderId)).Should().Be(1);
        (await assertionContext.StockMovements.CountAsync(x => x.ReferenceId == command.OrderId)).Should().Be(1);
        await _mediator.DidNotReceive().Publish(Arg.Any<StockReserved>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SameOrderWithDifferentItems_ThrowsConflict()
    {
        var variantId = Guid.NewGuid();
        await _dbContext.InventoryItems.AddAsync(InventoryTestFixture.CreateInventoryItem(variantId, onHand: 10));
        await _dbContext.SaveChangesAsync();

        var orderId = Guid.NewGuid();
        await CreateHandler(CreateFreshContext(), CreateLookupFor(variantId)).Handle(
            new ReserveStockCommand
            {
                OrderId = orderId,
                Items = [new ReserveStockItemRequest(variantId, 2)]
            },
            CancellationToken.None);

        var act = () => CreateHandler(CreateFreshContext(), CreateLookupFor(variantId)).Handle(
            new ReserveStockCommand
            {
                OrderId = orderId,
                Items = [new ReserveStockItemRequest(variantId, 3)]
            },
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictAppException>();
    }

    [Fact]
    public async Task Handle_InsufficientStock_ThrowsConflict()
    {
        var variantId = Guid.NewGuid();
        await _dbContext.InventoryItems.AddAsync(InventoryTestFixture.CreateInventoryItem(variantId, onHand: 2));
        await _dbContext.SaveChangesAsync();

        var act = () => CreateHandler(CreateFreshContext(), CreateLookupFor(variantId)).Handle(
            new ReserveStockCommand
            {
                OrderId = Guid.NewGuid(),
                Items = [new ReserveStockItemRequest(variantId, 3)]
            },
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictAppException>();
    }

    [Fact]
    public async Task Handle_UnknownVariant_ThrowsNotFound()
    {
        var variantId = Guid.NewGuid();
        await _dbContext.InventoryItems.AddAsync(InventoryTestFixture.CreateInventoryItem(variantId, onHand: 10));
        await _dbContext.SaveChangesAsync();

        var act = () => CreateHandler(CreateFreshContext(), InventoryTestFixture.CreateCatalogVariantLookup()).Handle(
            new ReserveStockCommand
            {
                OrderId = Guid.NewGuid(),
                Items = [new ReserveStockItemRequest(variantId, 1)]
            },
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundAppException>()
            .WithMessage($"*{variantId}*");
    }

    [Fact]
    public async Task Handle_InactiveVariant_ThrowsValidation()
    {
        var variantId = Guid.NewGuid();
        await _dbContext.InventoryItems.AddAsync(InventoryTestFixture.CreateInventoryItem(variantId, onHand: 10));
        await _dbContext.SaveChangesAsync();

        var act = () => CreateHandler(CreateFreshContext(), CreateInactiveLookup(variantId)).Handle(
            new ReserveStockCommand
            {
                OrderId = Guid.NewGuid(),
                Items = [new ReserveStockItemRequest(variantId, 1)]
            },
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationAppException>()
            .WithMessage($"*{variantId}*");
    }
}
