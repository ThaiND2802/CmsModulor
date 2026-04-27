using Commerce.Modules.Catalog.Application.Queries.GetProductBySlug;
using Commerce.Modules.Catalog.Domain;
using Commerce.Modules.Catalog.Tests.Common;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;

namespace Commerce.Modules.Catalog.Tests.Application.Queries.GetProductBySlug;

public sealed class GetProductBySlugHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsMappedProduct_WhenSlugMatchesAfterTrim()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var category = new Category { Id = Guid.NewGuid(), Name = "Shoes", Slug = "shoes" };
        var product = new Product { Id = Guid.NewGuid(), Name = "Air", Slug = "air", Sku = "SKU-1", CategoryId = category.Id, Category = category };
        dbContext.Categories.Add(category);
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();
        var handler = new GetProductBySlugHandler(dbContext, MapperFactory.Create());

        var response = await handler.Handle(new GetProductBySlugQuery(" air "), CancellationToken.None);

        response.Id.Should().Be(product.Id);
        response.Slug.Should().Be("air");
    }

    [Fact]
    public async Task Handle_ThrowsNotFound_WhenSlugIsBlank()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var handler = new GetProductBySlugHandler(dbContext, MapperFactory.Create());

        var act = () => handler.Handle(new GetProductBySlugQuery(" "), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundAppException>();
    }
}
