using Commerce.Modules.Catalog.Application.Queries.GetBrands;
using Commerce.Modules.Catalog.Domain;
using Commerce.Modules.Catalog.Tests.Common;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;

namespace Commerce.Modules.Catalog.Tests.Application.Queries.GetBrands;

public sealed class GetBrandsHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsPagedBrands_FilteredBySearch_OrderedByName()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        dbContext.Brands.AddRange(
            new Brand { Id = Guid.NewGuid(), Name = "Puma", Slug = "puma" },
            new Brand { Id = Guid.NewGuid(), Name = "Nike", Slug = "nike" },
            new Brand { Id = Guid.NewGuid(), Name = "Adidas", Slug = "adidas" });
        await dbContext.SaveChangesAsync();
        var handler = new GetBrandsHandler(dbContext, MapperFactory.Create());

        var response = await handler.Handle(new GetBrandsQuery { Search = "a", Page = 1, PageSize = 1 }, CancellationToken.None);

        response.Data.Should().ContainSingle();
        response.Data.First().Name.Should().Be("Adidas");
        response.Pagination!.Total.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ThrowsValidation_WhenPageSizeIsInvalid()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var handler = new GetBrandsHandler(dbContext, MapperFactory.Create());

        var act = () => handler.Handle(new GetBrandsQuery { Page = 1, PageSize = 0 }, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationAppException>();
    }
}
