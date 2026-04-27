using Commerce.Modules.Catalog.Contracts;
using Commerce.Modules.Inventory.Application.Commands.AdjustStock;
using Commerce.Modules.Inventory.Application.Events;
using Commerce.Modules.Inventory.Domain;
using Commerce.Modules.Inventory.Tests.TestCommon;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Commerce.Modules.Inventory.Tests.Application.Commands.AdjustStock;

public sealed class AdjustStockHandlerTests : IDisposable
{
    private readonly Commerce.Modules.Inventory.Infrastructure.InventoryDbContext _inventoryDbContext;
    private readonly IMediator _mediator;

    public AdjustStockHandlerTests()
    {
        _inventoryDbContext = InventoryTestFixture.CreateInventoryDbContext();
        _mediator = Substitute.For<IMediator>();
    }

    private AdjustStockHandler CreateHandler(ICatalogVariantLookup catalogVariantLookup) =>
        new(_inventoryDbContext, catalogVariantLookup, InventoryTestFixture.CreateMapper(), _mediator);

    public void Dispose()
    {
        _inventoryDbContext.Dispose();
    }

    [Fact]
    public async Task Handle_PositiveAdjustment_UpdatesStockAndCreatesMovement()
    {
        var variantId = Guid.NewGuid();
        var item = InventoryTestFixture.CreateInventoryItem(variantId, onHand: 10);
        await _inventoryDbContext.InventoryItems.AddAsync(item);
        await _inventoryDbContext.SaveChangesAsync();
        var command = InventoryTestFixture.CreateValidAdjustStockCommand(variantId) with { QuantityDelta = 4, Reason = "  Cycle count  " };

        var response = await CreateHandler(InventoryTestFixture.CreateCatalogVariantLookup(
            InventoryTestFixture.CreateCatalogVariantSummary(variantId))).Handle(command, CancellationToken.None);

        response.Status.Should().Be(200);
        response.Data.Should().NotBeNull();
        response.Data!.OnHandQuantity.Should().Be(14);
        response.Data.AvailableQuantity.Should().Be(14);

        var updated = await _inventoryDbContext.InventoryItems.FirstAsync(x => x.Id == item.Id);
        updated.OnHandQuantity.Should().Be(14);

        var movement = await _inventoryDbContext.StockMovements.FirstAsync(x => x.InventoryItemId == item.Id);
        movement.MovementType.Should().Be(StockMovementType.Adjusted);
        movement.QuantityDelta.Should().Be(4);
        movement.Reason.Should().Be("Cycle count");

        await _mediator.Received(1).Publish(
            Arg.Is<StockAdjusted>(x => x.InventoryItemId == item.Id && x.VariantId == variantId && x.QuantityDelta == 4),
            CancellationToken.None);
    }

    [Fact]
    public async Task Handle_StoresNormalizedReferenceType_OnMovement()
    {
        var variantId = Guid.NewGuid();
        var item = InventoryTestFixture.CreateInventoryItem(variantId, onHand: 10);
        await _inventoryDbContext.InventoryItems.AddAsync(item);
        await _inventoryDbContext.SaveChangesAsync();
        var command = InventoryTestFixture.CreateValidAdjustStockCommand(variantId) with
        {
            ReferenceType = "  manual-adjustment  "
        };

        await CreateHandler(InventoryTestFixture.CreateCatalogVariantLookup(
            InventoryTestFixture.CreateCatalogVariantSummary(variantId))).Handle(command, CancellationToken.None);

        var movement = await _inventoryDbContext.StockMovements.SingleAsync(x => x.InventoryItemId == item.Id);
        movement.ReferenceType.Should().Be("manual-adjustment");
    }

    [Fact]
    public async Task Handle_NegativeAdjustment_UpdatesStock()
    {
        var variantId = Guid.NewGuid();
        var item = InventoryTestFixture.CreateInventoryItem(variantId, onHand: 10);
        await _inventoryDbContext.InventoryItems.AddAsync(item);
        await _inventoryDbContext.SaveChangesAsync();
        var command = InventoryTestFixture.CreateValidAdjustStockCommand(variantId) with { QuantityDelta = -3 };

        var response = await CreateHandler(InventoryTestFixture.CreateCatalogVariantLookup(
            InventoryTestFixture.CreateCatalogVariantSummary(variantId))).Handle(command, CancellationToken.None);

        response.Data!.OnHandQuantity.Should().Be(7);

        var movement = await _inventoryDbContext.StockMovements.FirstAsync(x => x.InventoryItemId == item.Id);
        movement.QuantityDelta.Should().Be(-3);
        movement.OnHandAfter.Should().Be(7);
    }

    [Fact]
    public async Task Handle_MissingInventoryItem_ThrowsNotFound()
    {
        var command = InventoryTestFixture.CreateValidAdjustStockCommand();

        var act = () => CreateHandler(InventoryTestFixture.CreateCatalogVariantLookup()).Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundAppException>()
            .WithMessage($"*{command.VariantId}*");
    }

    [Fact]
    public async Task Handle_InactiveVariant_ThrowsValidation()
    {
        var variantId = Guid.NewGuid();
        var item = InventoryTestFixture.CreateInventoryItem(variantId, onHand: 10);
        await _inventoryDbContext.InventoryItems.AddAsync(item);
        await _inventoryDbContext.SaveChangesAsync();
        var command = InventoryTestFixture.CreateValidAdjustStockCommand(variantId);

        var act = () => CreateHandler(InventoryTestFixture.CreateCatalogVariantLookup(
            InventoryTestFixture.CreateCatalogVariantSummary(variantId, isActive: false))).Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationAppException>()
            .WithMessage($"*{variantId}*");
    }

    [Fact]
    public async Task Handle_MissingInventoryItem_ThrowsNotFound_WhenVariantExists()
    {
        var variantId = Guid.NewGuid();
        var command = InventoryTestFixture.CreateValidAdjustStockCommand(variantId);

        var act = () => CreateHandler(InventoryTestFixture.CreateCatalogVariantLookup(
            InventoryTestFixture.CreateCatalogVariantSummary(variantId))).Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundAppException>()
            .WithMessage("*Inventory item*");
    }

    [Fact]
    public async Task Handle_AdjustmentBelowReservedQuantity_ThrowsBusinessRuleException()
    {
        var variantId = Guid.NewGuid();
        var item = InventoryTestFixture.CreateInventoryItem(variantId, onHand: 10, reserved: 8);
        await _inventoryDbContext.InventoryItems.AddAsync(item);
        await _inventoryDbContext.SaveChangesAsync();
        var command = InventoryTestFixture.CreateValidAdjustStockCommand(variantId) with { QuantityDelta = -3 };

        var act = () => CreateHandler(InventoryTestFixture.CreateCatalogVariantLookup(
            InventoryTestFixture.CreateCatalogVariantSummary(variantId))).Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleAppException>()
            .WithMessage("*reserved quantity*");
    }
}
