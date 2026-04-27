using Commerce.Modules.Catalog.Application.Commands.AddProductMedia;
using Commerce.Modules.Catalog.Domain;
using Commerce.Modules.Catalog.Tests.Common;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Catalog.Tests.Application.Commands.AddProductMedia;

public sealed class AddProductMediaHandlerTests
{
    [Fact]
    public async Task Handle_AddsMedia_WithNextDisplayOrder_AndUnsetsExistingPrimary()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var product = new Product { Id = Guid.NewGuid(), Name = "Air", Slug = "air", Sku = "SKU-1", CategoryId = Guid.NewGuid() };
        dbContext.Categories.Add(new Category { Id = product.CategoryId, Name = "Shoes", Slug = "shoes" });
        dbContext.Products.Add(product);
        dbContext.ProductMedia.Add(new ProductMedia
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Product = product,
            Url = "https://example.com/old.jpg",
            MediaType = MediaType.Image,
            DisplayOrder = 3,
            IsPrimary = true
        });
        await dbContext.SaveChangesAsync();

        var handler = new AddProductMediaHandler(dbContext);
        var mediaId = await handler.Handle(new AddProductMediaCommand(product.Id, " https://example.com/new.jpg ", " Alt ", MediaType.Image, true), CancellationToken.None);

        var media = await dbContext.ProductMedia.SingleAsync(x => x.Id == mediaId);
        media.Url.Should().Be("https://example.com/new.jpg");
        media.AltText.Should().Be("Alt");
        media.DisplayOrder.Should().Be(4);
        media.IsPrimary.Should().BeTrue();
        (await dbContext.ProductMedia.SingleAsync(x => x.Url == "https://example.com/old.jpg")).IsPrimary.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ThrowsNotFound_WhenProductMissing()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var handler = new AddProductMediaHandler(dbContext);

        var act = () => handler.Handle(new AddProductMediaCommand(Guid.NewGuid(), "https://example.com/new.jpg", null, MediaType.Image, false), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundAppException>();
    }
}
