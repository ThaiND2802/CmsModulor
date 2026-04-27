using Commerce.Modules.Catalog.Application.Queries.GetBrandById;
using Commerce.Modules.Catalog.Domain;
using Commerce.Modules.Catalog.Tests.Common;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;

namespace Commerce.Modules.Catalog.Tests.Application.Queries.GetBrandById;

public sealed class GetBrandByIdHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsMappedBrand_WhenBrandExists()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var brand = new Brand { Id = Guid.NewGuid(), Name = "Nike", Slug = "nike" };
        dbContext.Brands.Add(brand);
        await dbContext.SaveChangesAsync();
        var handler = new GetBrandByIdHandler(dbContext, MapperFactory.Create());

        var response = await handler.Handle(new GetBrandByIdQuery(brand.Id), CancellationToken.None);

        response.Name.Should().Be("Nike");
    }

    [Fact]
    public async Task Handle_ThrowsNotFound_WhenBrandMissing()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var handler = new GetBrandByIdHandler(dbContext, MapperFactory.Create());

        var act = () => handler.Handle(new GetBrandByIdQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundAppException>();
    }
}
