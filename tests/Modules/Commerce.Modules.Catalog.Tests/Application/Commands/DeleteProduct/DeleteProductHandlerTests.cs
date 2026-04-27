using Commerce.Modules.Catalog.Application.Commands.DeleteProduct;
using Commerce.Modules.Catalog.Domain;
using Commerce.Modules.Catalog.Tests.Common;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Catalog.Tests.Application.Commands.DeleteProduct;

public sealed class DeleteProductHandlerTests
{
    [Fact]
    public async Task Handle_RemovesProduct_WhenFound()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var product = new Product { Id = Guid.NewGuid(), Name = "Air", Slug = "air", Sku = "SKU-1", CategoryId = Guid.NewGuid(), IsFeatured = true };
        dbContext.Categories.Add(new Category { Id = product.CategoryId, Name = "Shoes", Slug = "shoes" });
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();
        var handler = new DeleteProductHandler(dbContext);

        await handler.Handle(new DeleteProductCommand(product.Id), CancellationToken.None);

        var deleted = await dbContext.Products.IgnoreQueryFilters().SingleAsync(x => x.Id == product.Id);
        deleted.IsDeleted.Should().BeTrue();
        deleted.IsFeatured.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ThrowsNotFound_WhenMissing()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var handler = new DeleteProductHandler(dbContext);

        var act = () => handler.Handle(new DeleteProductCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundAppException>();
    }
}
