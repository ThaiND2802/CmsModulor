using Commerce.Modules.Catalog.Application.Commands.ChangeProductStatus;
using Commerce.Modules.Catalog.Domain;
using Commerce.Modules.Catalog.Tests.Common;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Catalog.Tests.Application.Commands.ChangeProductStatus;

public sealed class ChangeProductStatusHandlerTests
{
    [Fact]
    public async Task Handle_UpdatesStatus_WhenProductExists()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var product = new Product { Id = Guid.NewGuid(), Name = "Air", Slug = "air", Sku = "SKU-1", CategoryId = Guid.NewGuid(), Status = ProductStatus.Draft };
        dbContext.Categories.Add(new Category { Id = product.CategoryId, Name = "Shoes", Slug = "shoes" });
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();
        var handler = new ChangeProductStatusHandler(dbContext);

        await handler.Handle(new ChangeProductStatusCommand(product.Id, ProductStatus.Active), CancellationToken.None);

        (await dbContext.Products.SingleAsync(x => x.Id == product.Id)).Status.Should().Be(ProductStatus.Active);
    }

    [Fact]
    public async Task Handle_ThrowsNotFound_WhenProductMissing()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var handler = new ChangeProductStatusHandler(dbContext);

        var act = () => handler.Handle(new ChangeProductStatusCommand(Guid.NewGuid(), ProductStatus.Active), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundAppException>();
    }
}
