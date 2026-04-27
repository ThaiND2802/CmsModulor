using Commerce.Modules.Catalog.Application.Queries.GetProducts;
using Commerce.Modules.Catalog.Domain;
using Commerce.Modules.Catalog.Tests.Common;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;

namespace Commerce.Modules.Catalog.Tests.Application.Queries.GetProducts;

public sealed class GetProductsHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsAllProducts_WhenNoFilters()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var categoryId = await SeedProductsAsync(dbContext);
        var handler = new GetProductsHandler(dbContext, MapperFactory.Create());

        var response = await handler.Handle(new GetProductsQuery(), CancellationToken.None);

        response.Data.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_FiltersByCategoryId()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var categoryId = await SeedProductsAsync(dbContext);
        var handler = new GetProductsHandler(dbContext, MapperFactory.Create());

        var response = await handler.Handle(new GetProductsQuery { CategoryId = categoryId }, CancellationToken.None);

        response.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_FiltersByStatus()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        await SeedProductsAsync(dbContext);
        var handler = new GetProductsHandler(dbContext, MapperFactory.Create());

        var response = await handler.Handle(new GetProductsQuery { Status = ProductStatus.Active }, CancellationToken.None);

        response.Data.Should().OnlyContain(product => product.Status == ProductStatus.Active);
    }

    [Fact]
    public async Task Handle_PaginatesCorrectly()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        await SeedProductsAsync(dbContext);
        var handler = new GetProductsHandler(dbContext, MapperFactory.Create());

        var response = await handler.Handle(new GetProductsQuery { Page = 2, PageSize = 2, SortBy = "name", Desc = false }, CancellationToken.None);

        response.Data.Should().HaveCount(1);
        response.Pagination!.Page.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ThrowsValidation_WhenPageIsInvalid()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var handler = new GetProductsHandler(dbContext, MapperFactory.Create());

        var act = () => handler.Handle(new GetProductsQuery { Page = 0, PageSize = 20 }, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationAppException>();
    }

    private static async Task<Guid> SeedProductsAsync(Commerce.Modules.Catalog.Infrastructure.CatalogDbContext dbContext)
    {
        var categoryId = Guid.NewGuid();
        var otherCategoryId = Guid.NewGuid();

        var shoesCategory = new Category { Id = categoryId, Name = "Shoes", Slug = "shoes" };
        var bagsCategory = new Category { Id = otherCategoryId, Name = "Bags", Slug = "bags" };

        dbContext.Categories.AddRange(shoesCategory, bagsCategory);

        dbContext.Products.AddRange(
            new Product { Id = Guid.NewGuid(), Name = "Alpha", Slug = "alpha", Sku = "SKU-1", CategoryId = categoryId, Status = ProductStatus.Draft, CreatedAtUtc = new DateTime(2026, 4, 21, 0, 0, 0, DateTimeKind.Utc), Category = shoesCategory },
            new Product { Id = Guid.NewGuid(), Name = "Beta", Slug = "beta", Sku = "SKU-2", CategoryId = categoryId, Status = ProductStatus.Active, CreatedAtUtc = new DateTime(2026, 4, 22, 0, 0, 0, DateTimeKind.Utc), Category = shoesCategory },
            new Product { Id = Guid.NewGuid(), Name = "Gamma", Slug = "gamma", Sku = "SKU-3", CategoryId = otherCategoryId, Status = ProductStatus.Active, CreatedAtUtc = new DateTime(2026, 4, 23, 0, 0, 0, DateTimeKind.Utc), Category = bagsCategory });

        await dbContext.SaveChangesAsync();
        return categoryId;
    }
}
