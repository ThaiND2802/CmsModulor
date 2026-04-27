using Commerce.Modules.Inventory.Application.Queries.GetStockList;
using Commerce.Modules.Inventory.Tests.TestCommon;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;

namespace Commerce.Modules.Inventory.Tests.Application.Queries.GetStockList;

public sealed class GetStockListHandlerTests : IDisposable
{
    private readonly Commerce.Modules.Inventory.Infrastructure.InventoryDbContext _dbContext;
    private readonly GetStockListHandler _handler;

    public GetStockListHandlerTests()
    {
        _dbContext = InventoryTestFixture.CreateInventoryDbContext();
        _handler = new GetStockListHandler(_dbContext, InventoryTestFixture.CreateMapper());
    }

    public void Dispose() => _dbContext.Dispose();

    [Fact]
    public async Task Handle_ReturnsPagedStockItems()
    {
        await _dbContext.InventoryItems.AddRangeAsync(
            InventoryTestFixture.CreateInventoryItem(Guid.NewGuid(), "SKU-B", 8),
            InventoryTestFixture.CreateInventoryItem(Guid.NewGuid(), "SKU-A", 5));
        await _dbContext.SaveChangesAsync();

        var response = await _handler.Handle(new GetStockListQuery { Page = 1, PageSize = 10, SortBy = "sku" }, CancellationToken.None);

        response.Status.Should().Be(200);
        response.Data.Select(x => x.Sku).Should().Equal("SKU-A", "SKU-B");
        response.Pagination.Total.Should().Be(2);
    }

    [Fact]
    public async Task Handle_InvalidPage_ThrowsValidation()
    {
        var act = () => _handler.Handle(new GetStockListQuery { Page = 0, PageSize = 10 }, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationAppException>();
    }
}
