using Commerce.Modules.Catalog.Application.Commands.CreateProductVariant;
using Commerce.Modules.Catalog.Domain;
using Commerce.Modules.Catalog.Tests.Common;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Catalog.Tests.Application.Commands.CreateProductVariant;

public sealed class CreateProductVariantHandlerTests
{
    [Fact]
    public async Task Handle_CreatesVariant_AndUnsetsExistingDefault()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var category = new Category { Id = Guid.NewGuid(), Name = "Shoes", Slug = "shoes" };
        var product = new Product { Id = Guid.NewGuid(), Name = "Air", Slug = "air", Sku = "SKU-1", CategoryId = category.Id };
        var attribute = new ProductAttribute { Id = Guid.NewGuid(), Name = "color", DisplayName = "Color" };
        var existingVariant = new ProductVariant { Id = Guid.NewGuid(), ProductId = product.Id, Product = product, Sku = "SKU-OLD", Name = "Old", Price = 10m, StockQuantity = 1, IsDefault = true };
        dbContext.Categories.Add(category);
        dbContext.Products.Add(product);
        dbContext.ProductAttributes.Add(attribute);
        dbContext.ProductVariants.Add(existingVariant);
        await dbContext.SaveChangesAsync();
        var handler = new CreateProductVariantHandler(dbContext);

        var variantId = await handler.Handle(
            new CreateProductVariantCommand(product.Id, " SKU-NEW ", " Red ", 20m, null, 5, true, [new VariantOptionRequest(attribute.Id, " Red ")]),
            CancellationToken.None);

        var variant = await dbContext.ProductVariants.Include(x => x.Options).SingleAsync(x => x.Id == variantId);
        variant.Sku.Should().Be("SKU-NEW");
        variant.Name.Should().Be("Red");
        variant.IsDefault.Should().BeTrue();
        variant.Options.Should().ContainSingle(option => option.Value == "Red");
        (await dbContext.ProductVariants.SingleAsync(x => x.Id == existingVariant.Id)).IsDefault.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ThrowsNotFound_WhenAnyAttributeMissing()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var category = new Category { Id = Guid.NewGuid(), Name = "Shoes", Slug = "shoes" };
        var product = new Product { Id = Guid.NewGuid(), Name = "Air", Slug = "air", Sku = "SKU-1", CategoryId = category.Id };
        dbContext.Categories.Add(category);
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();
        var handler = new CreateProductVariantHandler(dbContext);

        var act = () => handler.Handle(
            new CreateProductVariantCommand(product.Id, "SKU-NEW", "Red", 20m, null, 5, false, [new VariantOptionRequest(Guid.NewGuid(), "Red")]),
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundAppException>();
    }
}
