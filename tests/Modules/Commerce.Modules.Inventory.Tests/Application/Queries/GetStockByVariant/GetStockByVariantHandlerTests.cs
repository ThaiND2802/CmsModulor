using Commerce.Modules.Inventory.Application.Queries.GetStockByVariant;
using Commerce.Modules.Inventory.Tests.TestCommon;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;

namespace Commerce.Modules.Inventory.Tests.Application.Queries.GetStockByVariant;

public sealed class GetStockByVariantHandlerTests : IDisposable
{
    private readonly Commerce.Modules.Inventory.Infrastructure.InventoryDbContext _dbContext;
    private readonly GetStockByVariantHandler _handler;

    public GetStockByVariantHandlerTests()
    {
        _dbContext = InventoryTestFixture.CreateInventoryDbContext();
        _handler = new GetStockByVariantHandler(_dbContext, InventoryTestFixture.CreateMapper());
    }

    public void Dispose() => _dbContext.Dispose();

    [Fact]
    public async Task Handle_ReturnsInventoryStock()
    {
        var variantId = Guid.NewGuid();
        var item = InventoryTestFixture.CreateInventoryItem(variantId, onHand: 11, reserved: 3);
        await _dbContext.InventoryItems.AddAsync(item);
        await _dbContext.SaveChangesAsync();

        var response = await _handler.Handle(new GetStockByVariantQuery(variantId), CancellationToken.None);

        response.Status.Should().Be(200);
        response.Data.Should().NotBeNull();
        response.Data!.VariantId.Should().Be(variantId);
        response.Data.AvailableQuantity.Should().Be(8);
    }

    [Fact]
    public async Task Handle_ThrowsNotFound_WhenInventoryMissing()
    {
        var act = () => _handler.Handle(new GetStockByVariantQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundAppException>();
    }
}
