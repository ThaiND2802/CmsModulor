using System.Diagnostics;
using Commerce.Modules.Catalog.Application.Commands.CreateProduct;
using Commerce.Modules.Catalog.Application.Commands.DeleteProduct;
using Commerce.Modules.Catalog.Application.Queries.GetProductById;
using Commerce.Modules.Catalog.Application.Queries.GetProducts;
using Commerce.Modules.Catalog.Domain;
using Commerce.Modules.Catalog.Tests.Common;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;

namespace Commerce.Modules.Catalog.Tests.Integration;

public sealed class CatalogIntegrationTests
{
    [Fact]
    public async Task CreateAndFetchProduct_WithVariantsAndMedia_Succeeds()
    {
        await RunWithPostgresAsync(async fixture =>
        {
            await fixture.ResetAsync();
            await using var dbContext = fixture.CreateDbContext();
            var mapper = MapperFactory.Create();

            var category = new Category { Id = Guid.NewGuid(), Name = "Shoes", Slug = "shoes" };
            var brand = new Brand { Id = Guid.NewGuid(), Name = "Nike", Slug = "nike" };
            var attribute = new ProductAttribute { Id = Guid.NewGuid(), Name = "color", DisplayName = "Color" };
            var product = new Product { Id = Guid.NewGuid(), Name = "Air", Slug = "air", Sku = "SKU-1", CategoryId = category.Id, Category = category, BrandId = brand.Id, Brand = brand };
            var variant = new ProductVariant { Id = Guid.NewGuid(), ProductId = product.Id, Product = product, Sku = "SKU-1-RED", Name = "Red", Price = 100, StockQuantity = 5, IsDefault = true };
            var media = new ProductMedia { Id = Guid.NewGuid(), ProductId = product.Id, Product = product, Url = "https://example.com/a.jpg", MediaType = MediaType.Image, IsPrimary = true };
            var option = new ProductVariantOption { Id = Guid.NewGuid(), VariantId = variant.Id, Variant = variant, AttributeId = attribute.Id, Attribute = attribute, Value = "Red" };

            product.Media.Add(media);
            product.Variants.Add(variant);
            variant.Options.Add(option);

            dbContext.Categories.Add(category);
            dbContext.Brands.Add(brand);
            dbContext.ProductAttributes.Add(attribute);
            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync();

            var handler = new GetProductByIdHandler(dbContext, mapper);
            var response = await handler.Handle(new GetProductByIdQuery(product.Id), CancellationToken.None);

            response.CategoryName.Should().Be("Shoes");
            response.BrandName.Should().Be("Nike");
            response.Media.Should().ContainSingle();
            response.Variants.Should().ContainSingle();
            response.Variants[0].Options.Should().ContainSingle();
        });
    }

    [Fact]
    public async Task SoftDeletedProduct_ExcludedFromGetProducts()
    {
        await RunWithPostgresAsync(async fixture =>
        {
            await fixture.ResetAsync();
            await using var dbContext = fixture.CreateDbContext();
            var mapper = MapperFactory.Create();

            var category = new Category { Id = Guid.NewGuid(), Name = "Shoes", Slug = "shoes" };
            var product = new Product { Id = Guid.NewGuid(), Name = "Air", Slug = "air", Sku = "SKU-1", CategoryId = category.Id, Category = category };
            var variant = new ProductVariant { Id = Guid.NewGuid(), ProductId = product.Id, Product = product, Sku = "SKU-1-RED", Name = "Red", Price = 100, StockQuantity = 5, IsDefault = true };

            product.Variants.Add(variant);
            dbContext.Categories.Add(category);
            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync();

            var deleteHandler = new DeleteProductHandler(dbContext);
            await deleteHandler.Handle(new DeleteProductCommand(product.Id), CancellationToken.None);

            var queryHandler = new GetProductsHandler(dbContext, mapper);
            var response = await queryHandler.Handle(new GetProductsQuery(), CancellationToken.None);

            response.Data.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task DuplicateSlug_ThrowsConflict()
    {
        await RunWithPostgresAsync(async fixture =>
        {
            await fixture.ResetAsync();
            await using var dbContext = fixture.CreateDbContext();

            var category = new Category { Id = Guid.NewGuid(), Name = "Shoes", Slug = "shoes" };
            dbContext.Categories.Add(category);
            await dbContext.SaveChangesAsync();

            var handler = new CreateProductHandler(dbContext);
            await handler.Handle(new CreateProductCommand("Air", "air", null, null, "SKU-1", category.Id, null, false, null), CancellationToken.None);

            var act = () => handler.Handle(new CreateProductCommand("Air 2", "air", null, null, "SKU-2", category.Id, null, false, null), CancellationToken.None);

            await act.Should().ThrowAsync<ConflictAppException>();
        });
    }

    private static async Task RunWithPostgresAsync(Func<PostgresCatalogFixture, Task> test)
    {
        if (!DockerIsAvailable())
        {
            return;
        }

        await using var fixture = new PostgresCatalogFixture();
        await fixture.InitializeAsync();
        await test(fixture);
    }

    private static bool DockerIsAvailable()
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = "info",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            if (process is null)
            {
                return false;
            }

            process.WaitForExit(5000);
            return !process.HasExited || process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
