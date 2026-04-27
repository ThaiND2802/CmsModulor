using AutoMapper;
using Commerce.Modules.Catalog.Contracts;
using Commerce.Modules.Inventory.Contracts;
using Commerce.Modules.Inventory.Contracts.Requests;
using Commerce.Modules.Inventory.Contracts.Responses;
using Commerce.Modules.Identity.Infrastructure;
using Commerce.Modules.Order.Contracts;
using Commerce.Modules.Sale.Application.Commands.CreateSale;
using Commerce.Modules.Sale.Application.Mappings;
using Commerce.Modules.Sale.Application.Services;
using Commerce.Modules.Sale.Domain;
using Commerce.Modules.Sale.Infrastructure;
using CommerceCore.Application.Abstractions;
using CommerceCore.FeatureManagement.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SaleEntity = Commerce.Modules.Sale.Domain.Sale;

namespace Commerce.Modules.Sale.Tests.TestCommon;

internal static class SaleTestFixture
{
    public static IMapper CreateMapper()
    {
        var configuration = new MapperConfiguration(config => config.AddProfile<SaleMappingProfile>(), NullLoggerFactory.Instance);
        configuration.AssertConfigurationIsValid();
        return configuration.CreateMapper();
    }

    public static SaleDbContext CreateSaleDbContext(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<SaleDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString("N"))
            .Options;

        return new SaleDbContext(options, CreateDateTimeProvider(), CreateCurrentUser());
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
        provider.UtcNow.Returns(new DateTime(2026, 4, 27, 0, 0, 0, DateTimeKind.Utc));
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

    public static CreateSaleCommand CreateValidCreateSaleCommand() =>
        new()
        {
            CustomerEmail = "buyer@example.com",
            CustomerPhone = "0123456789",
            Currency = "usd",
            ShippingAmount = 5m,
            DiscountAmount = 2m,
            TaxAmount = 1m,
            ShippingAddress = new CreateSaleAddressRequest(
                FullName: "John Doe",
                PhoneNumber: "0123456789",
                AddressLine1: "123 Main St",
                AddressLine2: null,
                City: "New York",
                State: "NY",
                PostalCode: "10001",
                Country: "USA"),
            BillingAddress = new CreateSaleAddressRequest(
                FullName: "John Doe",
                PhoneNumber: "0123456789",
                AddressLine1: "123 Main St",
                AddressLine2: null,
                City: "New York",
                State: "NY",
                PostalCode: "10001",
                Country: "USA"),
            Items =
            [
                new CreateSaleItemRequest(
                    ProductId: Guid.NewGuid(),
                    ProductName: "Test Product",
                    ProductSku: "TEST-SKU",
                    VariantId: null,
                    VariantName: null,
                    UnitPrice: 10m,
                    Quantity: 2,
                    DiscountAmount: 1m)
            ]
        };

    public static ISalePricingService CreateSalePricingService() => new DefaultSalePricingService();

    public static ISaleCouponService CreateSaleCouponService(params SaleCouponDefinition[] coupons)
    {
        var couponService = Substitute.For<ISaleCouponService>();
        var couponByCode = coupons.ToDictionary(x => x.Code, StringComparer.OrdinalIgnoreCase);
        couponService.GetByCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var code = callInfo.Arg<string>();
                return couponByCode.TryGetValue(code, out var coupon)
                    ? coupon
                    : null;
            });

        return couponService;
    }

    public static IPermissionGate CreatePermissionGate(bool allowed)
    {
        var permissionGate = Substitute.For<IPermissionGate>();
        permissionGate.HasPermissionAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(allowed);
        return permissionGate;
    }

    public static CatalogVariantSummary CreateCatalogVariantSummary(
        Guid? variantId = null,
        Guid? productId = null,
        string sku = "SALE-SKU-001",
        string productName = "Sale Product",
        string? variantName = "Default Variant",
        decimal unitPrice = 10m,
        bool isActive = true) =>
        new(
            variantId ?? Guid.NewGuid(),
            productId ?? Guid.NewGuid(),
            sku,
            productName,
            variantName,
            unitPrice,
            isActive);

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

    public static IInventoryModule CreateInventoryModule(
        InventoryAvailabilityResponse? reserveResponse = null,
        InventoryAvailabilityResponse? releaseResponse = null)
    {
        var inventoryModule = Substitute.For<IInventoryModule>();
        inventoryModule.ReserveStockAsync(Arg.Any<ReserveStockRequest>(), Arg.Any<CancellationToken>())
            .Returns(reserveResponse);
        inventoryModule.ReleaseStockAsync(Arg.Any<ReleaseStockRequest>(), Arg.Any<CancellationToken>())
            .Returns(releaseResponse);
        return inventoryModule;
    }

    public static IOrderModule CreateOrderModule(OrderCreationResponse? response = null)
    {
        var orderModule = Substitute.For<IOrderModule>();
        orderModule.CreateOrderFromSaleAsync(Arg.Any<CreateOrderFromSaleRequest>(), Arg.Any<CancellationToken>())
            .Returns(response ?? new OrderCreationResponse(true, Guid.NewGuid(), "ORD-TEST-001", null, null));
        return orderModule;
    }

    public static InventoryAvailabilityResponse CreateInventoryAvailabilityResponse(Guid reservationId, Guid ownerId, params InventoryAvailabilityItemResponse[] items) =>
        new(reservationId, ownerId, "Active", new DateTime(2026, 4, 27, 0, 0, 0, DateTimeKind.Utc), null, items);

    public static SaleEntity CreateSaleEntity(SaleStatus status = SaleStatus.Draft, string saleNumber = "SAL-20260427-0001") =>
        new()
        {
            Id = Guid.NewGuid(),
            SaleNumber = saleNumber,
            CustomerEmail = "buyer@example.com",
            CustomerPhone = "0123456789",
            Status = status,
            Currency = "USD",
            SubtotalAmount = 20m,
            DiscountAmount = 2m,
            ShippingAmount = 5m,
            TaxAmount = 1m,
            TotalAmount = 23m,
            CreatedAtUtc = new DateTime(2026, 4, 27, 0, 0, 0, DateTimeKind.Utc),
            ShippingAddress = new SaleAddress
            {
                FullName = "John Doe",
                PhoneNumber = "0123456789",
                AddressLine1 = "123 Main St",
                City = "New York",
                Country = "USA"
            },
            BillingAddress = new SaleAddress
            {
                FullName = "John Doe",
                PhoneNumber = "0123456789",
                AddressLine1 = "123 Main St",
                City = "New York",
                Country = "USA"
            }
        };

    public static SaleEntity CreatePricedSaleEntity(string saleNumber = "SAL-20260427-0001")
    {
        var sale = CreateSaleEntity(SaleStatus.Priced, saleNumber);
        sale.Items.Add(new SaleItem
        {
            Id = Guid.NewGuid(),
            SaleId = sale.Id,
            ProductId = Guid.NewGuid(),
            ProductName = "Product",
            ProductSku = "SKU-001",
            VariantId = Guid.NewGuid(),
            VariantName = "Default",
            UnitPrice = 10m,
            Quantity = 2,
            DiscountAmount = 0m,
            TotalAmount = 20m
        });
        sale.SubtotalAmount = 20m;
        sale.DiscountAmount = 0m;
        sale.ShippingAmount = 5m;
        sale.TaxAmount = 1m;
        sale.TotalAmount = 26m;
        sale.StatusHistory.Add(new SaleStatusHistory
        {
            Id = Guid.NewGuid(),
            SaleId = sale.Id,
            FromStatus = SaleStatus.Draft,
            ToStatus = SaleStatus.Priced,
            ChangedAtUtc = new DateTime(2026, 4, 27, 0, 0, 0, DateTimeKind.Utc)
        });
        return sale;
    }
}
