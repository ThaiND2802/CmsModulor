using Commerce.Modules.Catalog.Application.Queries.GetCategoryById;
using Commerce.Modules.Catalog.Domain;
using Commerce.Modules.Catalog.Tests.Common;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;

namespace Commerce.Modules.Catalog.Tests.Application.Queries.GetCategoryById;

public sealed class GetCategoryByIdHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsMappedCategory_WhenCategoryExists()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var category = new Category { Id = Guid.NewGuid(), Name = "Shoes", Slug = "shoes" };
        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync();
        var handler = new GetCategoryByIdHandler(dbContext, MapperFactory.Create());

        var response = await handler.Handle(new GetCategoryByIdQuery(category.Id), CancellationToken.None);

        response.Name.Should().Be("Shoes");
        response.Children.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ThrowsNotFound_WhenCategoryMissing()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var handler = new GetCategoryByIdHandler(dbContext, MapperFactory.Create());

        var act = () => handler.Handle(new GetCategoryByIdQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundAppException>();
    }
}
