using Commerce.Modules.Catalog.Application.Commands.DeleteBrand;
using Commerce.Modules.Catalog.Domain;
using Commerce.Modules.Catalog.Tests.Common;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Catalog.Tests.Application.Commands.DeleteBrand;

public sealed class DeleteBrandHandlerTests
{
    [Fact]
    public async Task Handle_RemovesBrand_WhenFound()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var brand = new Brand { Id = Guid.NewGuid(), Name = "Nike", Slug = "nike" };
        dbContext.Brands.Add(brand);
        await dbContext.SaveChangesAsync();
        var handler = new DeleteBrandHandler(dbContext);

        await handler.Handle(new DeleteBrandCommand(brand.Id), CancellationToken.None);

        (await dbContext.Brands.IgnoreQueryFilters().SingleAsync(x => x.Id == brand.Id)).IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ThrowsNotFound_WhenMissing()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var handler = new DeleteBrandHandler(dbContext);

        var act = () => handler.Handle(new DeleteBrandCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundAppException>();
    }
}
