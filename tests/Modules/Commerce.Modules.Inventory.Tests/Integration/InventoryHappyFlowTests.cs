using Commerce.Modules.Inventory.Application.Commands.ReceiveStock;
using Commerce.Modules.Inventory.Application.Commands.ReleaseStock;
using Commerce.Modules.Inventory.Application.Commands.ReserveStock;
using Commerce.Modules.Inventory.Application.Queries.GetStockByVariant;
using Commerce.Modules.Inventory.Application.Queries.GetStockMovements;
using Commerce.Modules.Inventory.Domain;
using Commerce.Modules.Inventory.Tests.TestCommon;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Commerce.Modules.Inventory.Tests.Integration;

public sealed class InventoryHappyFlowTests : IDisposable
{
    private readonly string _databaseName;
    private readonly Commerce.Modules.Inventory.Infrastructure.InventoryDbContext _dbContext;
    private readonly IMediator _mediator;

    public InventoryHappyFlowTests()
    {
        _databaseName = Guid.NewGuid().ToString();
        _dbContext = InventoryTestFixture.CreateInventoryDbContext(_databaseName);
        _mediator = Substitute.For<IMediator>();
    }

    public void Dispose() => _dbContext.Dispose();

    private Commerce.Modules.Inventory.Infrastructure.InventoryDbContext CreateFreshContext() =>
        InventoryTestFixture.CreateInventoryDbContext(_databaseName);

    [Fact]
    public async Task HappyFlow_ReceiveQueryReserveMovementsReleaseQuery_TransitionsStockAsExpected()
    {
        var variantId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var sku = "INV-HAPPY-001";
        var variant = InventoryTestFixture.CreateCatalogVariantSummary(variantId, sku);
        var catalogLookup = InventoryTestFixture.CreateCatalogVariantLookup(variant);

        var receiveHandler = new ReceiveStockHandler(CreateFreshContext(), catalogLookup, InventoryTestFixture.CreateMapper(), _mediator);
        var getStockHandler = new GetStockByVariantHandler(CreateFreshContext(), InventoryTestFixture.CreateMapper());
        var reserveHandler = new ReserveStockHandler(CreateFreshContext(), catalogLookup, InventoryTestFixture.CreateMapper(), _mediator);
        var getMovementsHandler = new GetStockMovementsHandler(CreateFreshContext(), InventoryTestFixture.CreateMapper());
        var releaseHandler = new ReleaseStockHandler(CreateFreshContext(), InventoryTestFixture.CreateMapper(), _mediator);

        var receiveResponse = await receiveHandler.Handle(
            InventoryTestFixture.CreateValidReceiveStockCommand(variantId) with
            {
                Sku = sku,
                Quantity = 10,
                Reason = "Initial stock"
            },
            CancellationToken.None);

        receiveResponse.Status.Should().Be(201);
        receiveResponse.Data.Should().NotBeNull();
        receiveResponse.Data!.VariantId.Should().Be(variantId);
        receiveResponse.Data.Sku.Should().Be(sku);
        receiveResponse.Data.OnHandQuantity.Should().Be(10);
        receiveResponse.Data.ReservedQuantity.Should().Be(0);
        receiveResponse.Data.AvailableQuantity.Should().Be(10);

        var stockAfterReceive = await getStockHandler.Handle(new GetStockByVariantQuery(variantId), CancellationToken.None);

        stockAfterReceive.Status.Should().Be(200);
        stockAfterReceive.Data.Should().NotBeNull();
        stockAfterReceive.Data!.OnHandQuantity.Should().Be(10);
        stockAfterReceive.Data.ReservedQuantity.Should().Be(0);
        stockAfterReceive.Data.AvailableQuantity.Should().Be(10);

        var reserveResponse = await reserveHandler.Handle(
            new ReserveStockCommand
            {
                OrderId = orderId,
                Items = [new ReserveStockItemRequest(variantId, 2)]
            },
            CancellationToken.None);

        reserveResponse.Status.Should().Be(201);
        reserveResponse.Data.Should().NotBeNull();
        reserveResponse.Data!.OrderId.Should().Be(orderId);
        reserveResponse.Data.Status.Should().Be(nameof(StockReservationStatus.Active));
        reserveResponse.Data.Items.Should().ContainSingle(x => x.VariantId == variantId && x.Quantity == 2);

        var stockAfterReserve = await getStockHandler.Handle(new GetStockByVariantQuery(variantId), CancellationToken.None);

        stockAfterReserve.Status.Should().Be(200);
        stockAfterReserve.Data.Should().NotBeNull();
        stockAfterReserve.Data!.OnHandQuantity.Should().Be(10);
        stockAfterReserve.Data.ReservedQuantity.Should().Be(2);
        stockAfterReserve.Data.AvailableQuantity.Should().Be(8);

        var movementsResponse = await getMovementsHandler.Handle(
            new GetStockMovementsQuery { VariantId = variantId, Page = 1, PageSize = 20 },
            CancellationToken.None);

        movementsResponse.Status.Should().Be(200);
        movementsResponse.Data.Should().HaveCount(2);
        var movementItems = movementsResponse.Data.ToList();
        movementItems.Select(x => x.MovementType).Should().Equal(nameof(StockMovementType.Reserved), nameof(StockMovementType.Received));
        movementItems[0].QuantityDelta.Should().Be(2);
        movementItems[0].ReservedAfter.Should().Be(2);
        movementItems[0].ReferenceId.Should().Be(orderId);
        movementItems[1].QuantityDelta.Should().Be(10);
        movementItems[1].OnHandAfter.Should().Be(10);

        var releaseResponse = await releaseHandler.Handle(new ReleaseStockCommand(orderId), CancellationToken.None);

        releaseResponse.Status.Should().Be(200);
        releaseResponse.Data.Should().NotBeNull();
        releaseResponse.Data!.Status.Should().Be(nameof(StockReservationStatus.Released));

        var stockAfterRelease = await getStockHandler.Handle(new GetStockByVariantQuery(variantId), CancellationToken.None);

        stockAfterRelease.Status.Should().Be(200);
        stockAfterRelease.Data.Should().NotBeNull();
        stockAfterRelease.Data!.OnHandQuantity.Should().Be(10);
        stockAfterRelease.Data.ReservedQuantity.Should().Be(0);
        stockAfterRelease.Data.AvailableQuantity.Should().Be(10);

        await using var assertionContext = CreateFreshContext();
        var movements = await assertionContext.StockMovements
            .Include(x => x.InventoryItem)
            .Where(x => x.InventoryItem.VariantId == variantId)
            .OrderBy(x => x.CreatedAtUtc)
            .ToListAsync();

        movements.Should().HaveCount(3);
        movements.Select(x => x.MovementType).Should().Equal(
            StockMovementType.Received,
            StockMovementType.Reserved,
            StockMovementType.Released);
        movements[2].QuantityDelta.Should().Be(-2);
        movements[2].ReservedAfter.Should().Be(0);
    }
}
