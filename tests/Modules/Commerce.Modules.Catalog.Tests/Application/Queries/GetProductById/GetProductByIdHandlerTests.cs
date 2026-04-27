using Commerce.Modules.Catalog.Application.Queries.GetProductById;
using Commerce.Modules.Catalog.Domain;
using Commerce.Modules.Catalog.Tests.Common;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;

namespace Commerce.Modules.Catalog.Tests.Application.Queries.GetProductById;

public sealed class GetProductByIdHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsMappedProduct_WhenProductExists()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var category = new Category { Id = Guid.NewGuid(), Name = "Shoes", Slug = "shoes" };
        var brand = new Brand { Id = Guid.NewGuid(), Name = "Nike", Slug = "nike" };
        var product = new Product { Id = Guid.NewGuid(), Name = "Air", Slug = "air", Sku = "SKU-1", CategoryId = category.Id, Category = category, BrandId = brand.Id, Brand = brand };
        dbContext.Categories.Add(category);
        dbContext.Brands.Add(brand);
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();
        var handler = new GetProductByIdHandler(dbContext, MapperFactory.Create());

        var response = await handler.Handle(new GetProductByIdQuery(product.Id), CancellationToken.None);

        response.Id.Should().Be(product.Id);
        response.CategoryName.Should().Be("Shoes");
        response.BrandName.Should().Be("Nike");
    }

    [Fact]
    public async Task Handle_ThrowsNotFound_WhenProductMissing()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var handler = new GetProductByIdHandler(dbContext, MapperFactory.Create());

        var act = () => handler.Handle(new GetProductByIdQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundAppException>();
    }
}
