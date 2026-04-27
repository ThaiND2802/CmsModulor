using Commerce.Modules.Catalog.Application.Commands.UpdateProductVariant;
using Commerce.Modules.Catalog.Domain;
using Commerce.Modules.Catalog.Tests.Common;
using CommerceCore.SharedKernel.Exceptions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Catalog.Tests.Application.Commands.UpdateProductVariant;

public sealed class UpdateProductVariantHandlerTests
{
    [Fact]
    public async Task Handle_UpdatesVariant_AndUnsetsOtherDefaults()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var current = new ProductVariant { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), Sku = "SKU-1", Name = "Base", Price = 10m, StockQuantity = 1, IsDefault = false, IsActive = true };
        var otherDefault = new ProductVariant { Id = Guid.NewGuid(), ProductId = current.ProductId, Sku = "SKU-2", Name = "Other", Price = 11m, StockQuantity = 2, IsDefault = true, IsActive = true };
        dbContext.ProductVariants.AddRange(current, otherDefault);
        await dbContext.SaveChangesAsync();
        var handler = new UpdateProductVariantHandler(dbContext);

        await handler.Handle(new UpdateProductVariantCommand(current.ProductId, current.Id, " SKU-3 ", " Updated ", 20m, 25m, 9, true, false), CancellationToken.None);

        var updated = await dbContext.ProductVariants.SingleAsync(x => x.Id == current.Id);
        updated.Sku.Should().Be("SKU-3");
        updated.Name.Should().Be("Updated");
        updated.Price.Should().Be(20m);
        updated.CompareAtPrice.Should().Be(25m);
        updated.StockQuantity.Should().Be(9);
        updated.IsDefault.Should().BeTrue();
        updated.IsActive.Should().BeFalse();
        (await dbContext.ProductVariants.SingleAsync(x => x.Id == otherDefault.Id)).IsDefault.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ThrowsConflict_WhenSkuAlreadyExists()
    {
        await using var dbContext = CatalogTestDbContextFactory.CreateInMemory();
        var current = new ProductVariant { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), Sku = "SKU-1", Name = "Base", Price = 10m, StockQuantity = 1 };
        var other = new ProductVariant { Id = Guid.NewGuid(), ProductId = current.ProductId, Sku = "SKU-2", Name = "Other", Price = 11m, StockQuantity = 2 };
        dbContext.ProductVariants.AddRange(current, other);
        await dbContext.SaveChangesAsync();
        var handler = new UpdateProductVariantHandler(dbContext);

        var act = () => handler.Handle(new UpdateProductVariantCommand(current.ProductId, current.Id, " SKU-2 ", "Updated", 20m, null, 9, false, true), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictAppException>();
    }
}
