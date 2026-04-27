using Commerce.Modules.Inventory.Application.Queries.GetStockMovements;
using Commerce.Modules.Inventory.Domain;
using Commerce.Modules.Inventory.Tests.TestCommon;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;

namespace Commerce.Modules.Inventory.Tests.Application.Queries.GetStockMovements;

public sealed class GetStockMovementsHandlerTests : IDisposable
{
    private readonly Commerce.Modules.Inventory.Infrastructure.InventoryDbContext _dbContext;
    private readonly GetStockMovementsHandler _handler;

    public GetStockMovementsHandlerTests()
    {
        _dbContext = InventoryTestFixture.CreateInventoryDbContext();
        _handler = new GetStockMovementsHandler(_dbContext, InventoryTestFixture.CreateMapper());
    }

    public void Dispose() => _dbContext.Dispose();

    [Fact]
    public async Task Handle_ReturnsVariantMovementsInDescendingOrder()
    {
        var variantId = Guid.NewGuid();
        var item = InventoryTestFixture.CreateInventoryItem(variantId, onHand: 10);
        var firstMovement = StockMovement.Create(item.Id, StockMovementType.Received, 5, 5, 0, "first");
        firstMovement.CreatedAtUtc = new DateTime(2026, 4, 24, 0, 0, 0, DateTimeKind.Utc);
        var secondMovement = StockMovement.Create(item.Id, StockMovementType.Received, 5, 10, 0, "second");
        secondMovement.CreatedAtUtc = new DateTime(2026, 4, 25, 0, 0, 0, DateTimeKind.Utc);

        await _dbContext.InventoryItems.AddAsync(item);
        await _dbContext.StockMovements.AddRangeAsync(firstMovement, secondMovement);
        await _dbContext.SaveChangesAsync();

        var response = await _handler.Handle(new GetStockMovementsQuery { VariantId = variantId, Page = 1, PageSize = 10 }, CancellationToken.None);

        response.Status.Should().Be(200);
        response.Data.Select(x => x.Reason).Should().Equal("second", "first");
        response.Data.Should().OnlyContain(x => x.VariantId == variantId);
    }

    [Fact]
    public async Task Handle_ThrowsNotFound_WhenInventoryMissing()
    {
        var act = () => _handler.Handle(new GetStockMovementsQuery { VariantId = Guid.NewGuid(), Page = 1, PageSize = 10 }, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundAppException>();
    }
}
