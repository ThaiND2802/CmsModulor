using Commerce.Modules.Catalog.Application.Queries.GetProductVariants;
using Commerce.Modules.Catalog.Domain;
using Commerce.Modules.Catalog.Tests.Common;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;

namespace Commerce.Modules.Catalog.Tests.Application.Queries.GetProductVariants;

public sealed class GetProductVariantsHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsVariants_OrderedByDefaultThenName()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var product = new Product { Id = Guid.NewGuid(), Name = "Air", Slug = "air", Sku = "SKU-1", CategoryId = Guid.NewGuid() };
        dbContext.Categories.Add(new Category { Id = product.CategoryId, Name = "Shoes", Slug = "shoes" });
        dbContext.Products.Add(product);
        dbContext.ProductVariants.AddRange(
            new ProductVariant { Id = Guid.NewGuid(), ProductId = product.Id, Product = product, Sku = "SKU-B", Name = "Beta", Price = 10m, StockQuantity = 1, IsDefault = false },
            new ProductVariant { Id = Guid.NewGuid(), ProductId = product.Id, Product = product, Sku = "SKU-A", Name = "Alpha", Price = 11m, StockQuantity = 2, IsDefault = true });
        await dbContext.SaveChangesAsync();
        var handler = new GetProductVariantsHandler(dbContext, MapperFactory.Create());

        var response = await handler.Handle(new GetProductVariantsQuery(product.Id), CancellationToken.None);

        response.Select(x => x.Name).Should().Equal("Alpha", "Beta");
    }

    [Fact]
    public async Task Handle_ThrowsNotFound_WhenProductMissing()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var handler = new GetProductVariantsHandler(dbContext, MapperFactory.Create());

        var act = () => handler.Handle(new GetProductVariantsQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundAppException>();
    }
}
