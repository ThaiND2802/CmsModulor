using Commerce.Modules.Catalog.Contracts;
using Commerce.Modules.Inventory.Application.Commands.ReceiveStock;
using Commerce.Modules.Inventory.Application.Events;
using Commerce.Modules.Inventory.Domain;
using Commerce.Modules.Inventory.Tests.TestCommon;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Commerce.Modules.Inventory.Tests.Application.Commands.ReceiveStock;

public sealed class ReceiveStockHandlerTests : IDisposable
{
    private readonly Commerce.Modules.Inventory.Infrastructure.InventoryDbContext _inventoryDbContext;
    private readonly IMediator _mediator;

    public ReceiveStockHandlerTests()
    {
        _inventoryDbContext = InventoryTestFixture.CreateInventoryDbContext();
        _mediator = Substitute.For<IMediator>();
    }

    public void Dispose()
    {
        _inventoryDbContext.Dispose();
    }

    private ReceiveStockHandler CreateHandler(ICatalogVariantLookup catalogVariantLookup) =>
        new(_inventoryDbContext, catalogVariantLookup, InventoryTestFixture.CreateMapper(), _mediator);

    [Fact]
    public async Task Handle_ValidCommand_CreatesInventoryItemAndPublishesEvent()
    {
        var variant = InventoryTestFixture.CreateCatalogVariantSummary();
        var command = InventoryTestFixture.CreateValidReceiveStockCommand(variant.Id);

        var response = await CreateHandler(InventoryTestFixture.CreateCatalogVariantLookup(variant)).Handle(command, CancellationToken.None);

        response.Status.Should().Be(201);
        response.Data.Should().NotBeNull();
        response.Data!.OnHandQuantity.Should().Be(command.Quantity);
        response.Data.AvailableQuantity.Should().Be(command.Quantity);

        var item = await _inventoryDbContext.InventoryItems.FirstOrDefaultAsync(x => x.VariantId == variant.Id);
        item.Should().NotBeNull();
        item!.Sku.Should().Be(variant.Sku);
        item.OnHandQuantity.Should().Be(command.Quantity);

        var movement = await _inventoryDbContext.StockMovements.FirstOrDefaultAsync(x => x.InventoryItemId == item.Id);
        movement.Should().NotBeNull();
        movement!.MovementType.Should().Be(StockMovementType.Received);
        movement.QuantityDelta.Should().Be(command.Quantity);

        await _mediator.Received(1).Publish(
            Arg.Is<StockReceived>(x => x.InventoryItemId == item.Id && x.VariantId == variant.Id && x.QuantityReceived == command.Quantity),
            CancellationToken.None);
    }

    [Fact]
    public async Task Handle_ExistingInventoryItem_IncrementsOnHandQuantity()
    {
        var variant = InventoryTestFixture.CreateCatalogVariantSummary();
        var item = InventoryTestFixture.CreateInventoryItem(variant.Id, onHand: 7);
        await _inventoryDbContext.InventoryItems.AddAsync(item);
        await _inventoryDbContext.SaveChangesAsync();
        var command = InventoryTestFixture.CreateValidReceiveStockCommand(variant.Id) with { Quantity = 3, Sku = "  UPDATED-SKU  ", Reason = "  Restock  " };

        var response = await CreateHandler(InventoryTestFixture.CreateCatalogVariantLookup(variant)).Handle(command, CancellationToken.None);

        response.Data!.OnHandQuantity.Should().Be(10);
        var updated = await _inventoryDbContext.InventoryItems.FirstAsync(x => x.Id == item.Id);
        updated.Sku.Should().Be(variant.Sku);
        updated.OnHandQuantity.Should().Be(10);

        var movement = await _inventoryDbContext.StockMovements.OrderByDescending(x => x.CreatedAtUtc).FirstAsync();
        movement.Reason.Should().Be("Restock");
    }

    [Fact]
    public async Task Handle_StoresNormalizedReferenceType_OnMovement()
    {
        var variant = InventoryTestFixture.CreateCatalogVariantSummary();
        var command = InventoryTestFixture.CreateValidReceiveStockCommand(variant.Id) with
        {
            ReferenceType = "  purchase-order  "
        };

        await CreateHandler(InventoryTestFixture.CreateCatalogVariantLookup(variant)).Handle(command, CancellationToken.None);

        var movement = await _inventoryDbContext.StockMovements.SingleAsync();
        movement.ReferenceType.Should().Be("purchase-order");
    }

    [Fact]
    public async Task Handle_UsesCatalogSku_AsCanonicalValue()
    {
        var variant = InventoryTestFixture.CreateCatalogVariantSummary(sku: "CATALOG-SKU");
        var command = InventoryTestFixture.CreateValidReceiveStockCommand(variant.Id) with { Sku = "REQUEST-SKU" };

        var response = await CreateHandler(InventoryTestFixture.CreateCatalogVariantLookup(variant)).Handle(command, CancellationToken.None);

        response.Data!.Sku.Should().Be("CATALOG-SKU");
        var item = await _inventoryDbContext.InventoryItems.FirstAsync(x => x.VariantId == variant.Id);
        item.Sku.Should().Be("CATALOG-SKU");
    }

    [Fact]
    public async Task Handle_MissingVariant_ThrowsNotFound()
    {
        var command = InventoryTestFixture.CreateValidReceiveStockCommand();

        var act = () => CreateHandler(InventoryTestFixture.CreateCatalogVariantLookup()).Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundAppException>();
    }

    [Fact]
    public async Task Handle_InactiveVariant_ThrowsValidation()
    {
        var variant = InventoryTestFixture.CreateCatalogVariantSummary(isActive: false);
        var command = InventoryTestFixture.CreateValidReceiveStockCommand(variant.Id);

        var act = () => CreateHandler(InventoryTestFixture.CreateCatalogVariantLookup(variant)).Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationAppException>()
            .WithMessage($"*{variant.Id}*");
    }
}
