using AutoMapper;
using Commerce.Modules.Catalog.Contracts;
using Commerce.Modules.Catalog.Domain;
using Commerce.Modules.Catalog.Infrastructure;
using Commerce.Modules.Inventory.Application.Commands.AdjustStock;
using Commerce.Modules.Inventory.Application.Commands.ReceiveStock;
using Commerce.Modules.Inventory.Application.Commands.ReserveStock;
using Commerce.Modules.Inventory.Application.Mappings;
using Commerce.Modules.Inventory.Domain;
using Commerce.Modules.Inventory.Infrastructure;
using Commerce.Modules.Identity.Infrastructure;
using CommerceCore.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Commerce.Modules.Inventory.Tests.TestCommon;

internal static class InventoryTestFixture
{
    public static IMapper CreateMapper()
    {
        var configuration = new MapperConfiguration(config => config.AddProfile<InventoryMappingProfile>(), NullLoggerFactory.Instance);
        configuration.AssertConfigurationIsValid();
        return configuration.CreateMapper();
    }

    public static InventoryDbContext CreateInventoryDbContext(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString("N"))
            .Options;

        return new InventoryDbContext(options, CreateDateTimeProvider(), CreateCurrentUser());
    }

    public static CatalogDbContext CreateCatalogDbContext(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString("N"))
            .Options;

        return new CatalogDbContext(options, CreateDateTimeProvider(), CreateCurrentUser());
    }

    public static IdentityDbContext CreateIdentityDbContext(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString("N"))
            .Options;

        return new IdentityDbContext(options, CreateDateTimeProvider(), CreateCurrentUser());
    }

    public static IDateTimeProvider CreateDateTimeProvider()
    {
        var provider = Substitute.For<IDateTimeProvider>();
        provider.UtcNow.Returns(new DateTime(2026, 4, 25, 0, 0, 0, DateTimeKind.Utc));
        return provider;
    }

    public static ICurrentUser CreateCurrentUser(string? userId = null, string? userName = null)
    {
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(userId ?? Guid.NewGuid().ToString());
        currentUser.UserName.Returns(userName ?? "test-user");
        currentUser.IsAuthenticated.Returns(true);
        return currentUser;
    }

    public static ReceiveStockCommand CreateValidReceiveStockCommand(Guid? variantId = null) =>
        new()
        {
            VariantId = variantId ?? Guid.NewGuid(),
            Sku = "INV-SKU-001",
            Quantity = 5,
            Reason = "Initial stock",
            ReferenceType = "purchase-order",
            ReferenceId = Guid.NewGuid()
        };

    public static AdjustStockCommand CreateValidAdjustStockCommand(Guid? variantId = null) =>
        new()
        {
            VariantId = variantId ?? Guid.NewGuid(),
            QuantityDelta = 2,
            Reason = "Cycle count adjustment",
            ReferenceType = "manual-adjustment",
            ReferenceId = Guid.NewGuid()
        };

    public static ReserveStockCommand CreateValidReserveStockCommand(Guid orderId, params ReserveStockItemRequest[] items) =>
        new()
        {
            OrderId = orderId,
            Items = items.Length == 0
                ? [new ReserveStockItemRequest(Guid.NewGuid(), 1)]
                : items
        };

    public static ProductVariant CreateVariant(Guid? id = null, string sku = "INV-SKU-001") =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            Sku = sku,
            Name = "Variant",
            Price = 10m,
            StockQuantity = 0,
            IsDefault = true,
            IsActive = true
        };

    public static CatalogVariantSummary CreateCatalogVariantSummary(Guid? id = null, string sku = "INV-SKU-001", bool isActive = true) =>
        new(id ?? Guid.NewGuid(), sku, isActive);

    public static ICatalogVariantLookup CreateCatalogVariantLookup(params CatalogVariantSummary[] variants)
    {
        var lookup = Substitute.For<ICatalogVariantLookup>();
        var variantsById = variants.ToDictionary(x => x.Id);

        lookup.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var variantId = callInfo.Arg<Guid>();
                return variantsById.TryGetValue(variantId, out var variant)
                    ? variant
                    : null;
            });

        return lookup;
    }

    public static InventoryItem CreateInventoryItem(Guid variantId, string sku = "INV-SKU-001", int onHand = 10, int reserved = 0)
    {
        var item = InventoryItem.Create(variantId, sku, onHand);
        if (reserved > 0)
        {
            item.Reserve(reserved);
        }

        return item;
    }

}
