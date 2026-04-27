using Commerce.Modules.Catalog.Application.Commands.CreateProduct;
using Commerce.Modules.Catalog.Domain;
using Commerce.Modules.Catalog.Tests.Common;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Catalog.Tests.Application.Commands.CreateProduct;

public sealed class CreateProductHandlerTests
{
    [Fact]
    public async Task Handle_CreatesProduct_WithDraftStatus()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var categoryId = Guid.NewGuid();
        dbContext.Categories.Add(new Category { Id = categoryId, Name = "Shoes", Slug = "shoes" });
        await dbContext.SaveChangesAsync();

        var handler = new CreateProductHandler(dbContext);
        var command = new CreateProductCommand("Sneaker", "sneaker", null, null, "SKU-1", categoryId, null, false, null);

        var productId = await handler.Handle(command, CancellationToken.None);

        var product = await dbContext.Products.SingleAsync(x => x.Id == productId);
        product.Status.Should().Be(ProductStatus.Draft);
    }

    [Fact]
    public async Task Handle_ThrowsNotFound_WhenCategoryMissing()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var handler = new CreateProductHandler(dbContext);
        var command = new CreateProductCommand("Sneaker", "sneaker", null, null, "SKU-1", Guid.NewGuid(), null, false, null);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundAppException>();
    }

    [Fact]
    public async Task Handle_ThrowsConflict_WhenSlugAlreadyExists()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var categoryId = Guid.NewGuid();
        dbContext.Categories.Add(new Category { Id = categoryId, Name = "Shoes", Slug = "shoes" });
        dbContext.Products.Add(new Product { Id = Guid.NewGuid(), Name = "Existing", Slug = "sneaker", Sku = "SKU-0", CategoryId = categoryId });
        await dbContext.SaveChangesAsync();

        var handler = new CreateProductHandler(dbContext);
        var command = new CreateProductCommand("Sneaker", "sneaker", null, null, "SKU-1", categoryId, null, false, null);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictAppException>();
    }
}
