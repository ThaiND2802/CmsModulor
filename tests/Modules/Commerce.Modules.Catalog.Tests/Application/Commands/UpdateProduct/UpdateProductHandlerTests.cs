using Commerce.Modules.Catalog.Application.Commands.UpdateProduct;
using Commerce.Modules.Catalog.Domain;
using Commerce.Modules.Catalog.Tests.Common;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Catalog.Tests.Application.Commands.UpdateProduct;

public sealed class UpdateProductHandlerTests
{
    [Fact]
    public async Task Handle_UpdatesProduct_WhenValid()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var category = new Category { Id = Guid.NewGuid(), Name = "Shoes", Slug = "shoes" };
        var brand = new Brand { Id = Guid.NewGuid(), Name = "Nike", Slug = "nike" };
        var product = new Product { Id = Guid.NewGuid(), Name = "Air", Slug = "air", Sku = "SKU-1", CategoryId = category.Id };
        dbContext.Categories.Add(category);
        dbContext.Brands.Add(brand);
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();
        var handler = new UpdateProductHandler(dbContext);

        await handler.Handle(new UpdateProductCommand(product.Id, " Air Max ", " air-max ", " Desc ", " Short ", " SKU-2 ", category.Id, brand.Id, true, " sport "), CancellationToken.None);

        var updated = await dbContext.Products.SingleAsync(x => x.Id == product.Id);
        updated.Name.Should().Be("Air Max");
        updated.Slug.Should().Be("air-max");
        updated.Description.Should().Be("Desc");
        updated.ShortDescription.Should().Be("Short");
        updated.Sku.Should().Be("SKU-2");
        updated.BrandId.Should().Be(brand.Id);
        updated.IsFeatured.Should().BeTrue();
        updated.Tags.Should().Be("sport");
    }

    [Fact]
    public async Task Handle_ThrowsConflict_WhenSkuAlreadyExists()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var category = new Category { Id = Guid.NewGuid(), Name = "Shoes", Slug = "shoes" };
        var product = new Product { Id = Guid.NewGuid(), Name = "Air", Slug = "air", Sku = "SKU-1", CategoryId = category.Id };
        var other = new Product { Id = Guid.NewGuid(), Name = "Zoom", Slug = "zoom", Sku = "SKU-2", CategoryId = category.Id };
        dbContext.Categories.Add(category);
        dbContext.Products.AddRange(product, other);
        await dbContext.SaveChangesAsync();
        var handler = new UpdateProductHandler(dbContext);

        var act = () => handler.Handle(new UpdateProductCommand(product.Id, "Air", "air", null, null, " SKU-2 ", category.Id, null, false, null), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictAppException>();
    }
}
