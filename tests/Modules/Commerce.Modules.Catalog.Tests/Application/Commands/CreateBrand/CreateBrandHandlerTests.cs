using Commerce.Modules.Catalog.Application.Commands.CreateBrand;
using Commerce.Modules.Catalog.Domain;
using Commerce.Modules.Catalog.Tests.Common;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Catalog.Tests.Application.Commands.CreateBrand;

public sealed class CreateBrandHandlerTests
{
    [Fact]
    public async Task Handle_CreatesBrand_WhenSlugIsUnique()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var handler = new CreateBrandHandler(dbContext);

        var brandId = await handler.Handle(new CreateBrandCommand(" Nike ", " nike ", " https://example.com/logo.png ", " https://example.com ", false), CancellationToken.None);

        var brand = await dbContext.Brands.SingleAsync(x => x.Id == brandId);
        brand.Name.Should().Be("Nike");
        brand.Slug.Should().Be("nike");
        brand.LogoUrl.Should().Be("https://example.com/logo.png");
        brand.Website.Should().Be("https://example.com");
        brand.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ThrowsConflict_WhenSlugAlreadyExists()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        dbContext.Brands.Add(new Brand { Id = Guid.NewGuid(), Name = "Existing", Slug = "nike" });
        await dbContext.SaveChangesAsync();
        var handler = new CreateBrandHandler(dbContext);

        var act = () => handler.Handle(new CreateBrandCommand("Nike", " nike ", null, null), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictAppException>();
    }
}
