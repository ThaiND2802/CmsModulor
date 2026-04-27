using Commerce.Modules.Catalog.Application.Commands.DeleteProductVariant;
using Commerce.Modules.Catalog.Domain;
using Commerce.Modules.Catalog.Tests.Common;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Catalog.Tests.Application.Commands.DeleteProductVariant;

public sealed class DeleteProductVariantHandlerTests
{
    [Fact]
    public async Task Handle_RemovesVariant_WhenAllowed()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var product = new Product { Id = Guid.NewGuid(), Name = "Air", Slug = "air", Sku = "SKU-1", CategoryId = Guid.NewGuid(), Status = ProductStatus.Draft };
        var first = new ProductVariant { Id = Guid.NewGuid(), ProductId = product.Id, Product = product, Sku = "SKU-1-A", Name = "A", Price = 10m, StockQuantity = 1 };
        var second = new ProductVariant { Id = Guid.NewGuid(), ProductId = product.Id, Product = product, Sku = "SKU-1-B", Name = "B", Price = 11m, StockQuantity = 2 };
        dbContext.Categories.Add(new Category { Id = product.CategoryId, Name = "Shoes", Slug = "shoes" });
        dbContext.Products.Add(product);
        dbContext.ProductVariants.AddRange(first, second);
        await dbContext.SaveChangesAsync();
        var handler = new DeleteProductVariantHandler(dbContext);

        await handler.Handle(new DeleteProductVariantCommand(product.Id, first.Id), CancellationToken.None);

        (await dbContext.ProductVariants.AnyAsync(x => x.Id == first.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ThrowsConflict_WhenDeletingLastVariantOfActiveProduct()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var product = new Product { Id = Guid.NewGuid(), Name = "Air", Slug = "air", Sku = "SKU-1", CategoryId = Guid.NewGuid(), Status = ProductStatus.Active };
        var variant = new ProductVariant { Id = Guid.NewGuid(), ProductId = product.Id, Product = product, Sku = "SKU-1-A", Name = "A", Price = 10m, StockQuantity = 1 };
        dbContext.Categories.Add(new Category { Id = product.CategoryId, Name = "Shoes", Slug = "shoes" });
        dbContext.Products.Add(product);
        dbContext.ProductVariants.Add(variant);
        await dbContext.SaveChangesAsync();
        var handler = new DeleteProductVariantHandler(dbContext);

        var act = () => handler.Handle(new DeleteProductVariantCommand(product.Id, variant.Id), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictAppException>();
    }
}
