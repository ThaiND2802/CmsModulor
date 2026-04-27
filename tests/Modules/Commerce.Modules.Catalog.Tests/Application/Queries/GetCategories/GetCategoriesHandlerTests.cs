using Commerce.Modules.Catalog.Application.Queries.GetCategories;
using Commerce.Modules.Catalog.Domain;
using Commerce.Modules.Catalog.Tests.Common;
using FluentAssertions;

namespace Commerce.Modules.Catalog.Tests.Application.Queries.GetCategories;

public sealed class GetCategoriesHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsTree_WhenTreeViewEnabled()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var parent = new Category { Id = Guid.NewGuid(), Name = "Shoes", Slug = "shoes" };
        var child = new Category { Id = Guid.NewGuid(), Name = "Boots", Slug = "boots", ParentId = parent.Id };
        dbContext.Categories.AddRange(parent, child);
        await dbContext.SaveChangesAsync();
        var handler = new GetCategoriesHandler(dbContext, MapperFactory.Create());

        var response = await handler.Handle(new GetCategoriesQuery(true), CancellationToken.None);

        response.Should().ContainSingle();
        response[0].Children.Should().ContainSingle(x => x.Id == child.Id);
    }

    [Fact]
    public async Task Handle_ReturnsFlatList_WhenTreeViewDisabled()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var parent = new Category { Id = Guid.NewGuid(), Name = "Shoes", Slug = "shoes" };
        var child = new Category { Id = Guid.NewGuid(), Name = "Boots", Slug = "boots", ParentId = parent.Id };
        dbContext.Categories.AddRange(parent, child);
        await dbContext.SaveChangesAsync();
        var handler = new GetCategoriesHandler(dbContext, MapperFactory.Create());

        var response = await handler.Handle(new GetCategoriesQuery(false), CancellationToken.None);

        response.Should().HaveCount(2);
        response.Should().OnlyContain(x => x.Children.Count == 0);
    }
}
