using Commerce.Modules.Catalog.Application.Commands.RemoveProductMedia;
using Commerce.Modules.Catalog.Domain;
using Commerce.Modules.Catalog.Tests.Common;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Catalog.Tests.Application.Commands.RemoveProductMedia;

public sealed class RemoveProductMediaHandlerTests
{
    [Fact]
    public async Task Handle_RemovesMedia_WhenFound()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var product = new Product { Id = Guid.NewGuid(), Name = "Air", Slug = "air", Sku = "SKU-1", CategoryId = Guid.NewGuid() };
        var media = new ProductMedia { Id = Guid.NewGuid(), ProductId = product.Id, Product = product, Url = "https://example.com/a.jpg", MediaType = MediaType.Image };
        dbContext.Categories.Add(new Category { Id = product.CategoryId, Name = "Shoes", Slug = "shoes" });
        dbContext.Products.Add(product);
        dbContext.ProductMedia.Add(media);
        await dbContext.SaveChangesAsync();
        var handler = new RemoveProductMediaHandler(dbContext);

        await handler.Handle(new RemoveProductMediaCommand(product.Id, media.Id), CancellationToken.None);

        (await dbContext.ProductMedia.AnyAsync(x => x.Id == media.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ThrowsNotFound_WhenMissing()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var handler = new RemoveProductMediaHandler(dbContext);

        var act = () => handler.Handle(new RemoveProductMediaCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundAppException>();
    }
}
