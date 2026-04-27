using Commerce.Modules.Catalog.Application.Commands.UpdateBrand;
using Commerce.Modules.Catalog.Domain;
using Commerce.Modules.Catalog.Tests.Common;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Catalog.Tests.Application.Commands.UpdateBrand;

public sealed class UpdateBrandHandlerTests
{
    [Fact]
    public async Task Handle_UpdatesBrand_WhenValid()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var brand = new Brand { Id = Guid.NewGuid(), Name = "Nike", Slug = "nike" };
        dbContext.Brands.Add(brand);
        await dbContext.SaveChangesAsync();
        var handler = new UpdateBrandHandler(dbContext);

        await handler.Handle(new UpdateBrandCommand(brand.Id, " Nike Updated ", " nike-updated ", " https://example.com/logo.png ", " https://example.com ", false), CancellationToken.None);

        var updated = await dbContext.Brands.SingleAsync(x => x.Id == brand.Id);
        updated.Name.Should().Be("Nike Updated");
        updated.Slug.Should().Be("nike-updated");
        updated.LogoUrl.Should().Be("https://example.com/logo.png");
        updated.Website.Should().Be("https://example.com");
        updated.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ThrowsConflict_WhenSlugAlreadyExists()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var brand = new Brand { Id = Guid.NewGuid(), Name = "Nike", Slug = "nike" };
        var other = new Brand { Id = Guid.NewGuid(), Name = "Puma", Slug = "puma" };
        dbContext.Brands.AddRange(brand, other);
        await dbContext.SaveChangesAsync();
        var handler = new UpdateBrandHandler(dbContext);

        var act = () => handler.Handle(new UpdateBrandCommand(brand.Id, "Nike", " puma ", null, null, true), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictAppException>();
    }
}
