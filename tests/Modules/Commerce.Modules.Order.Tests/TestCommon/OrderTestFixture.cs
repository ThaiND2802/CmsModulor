using AutoMapper;
using Commerce.Modules.Inventory.Contracts;
using Commerce.Modules.Inventory.Contracts.Requests;
using Commerce.Modules.Inventory.Contracts.Responses;
using Commerce.Modules.Order.Application.Commands.CreateOrder;
using Commerce.Modules.Order.Application.Mappings;
using Commerce.Modules.Order.Domain;
using Commerce.Modules.Order.Infrastructure;
using OrderEntity = Commerce.Modules.Order.Domain.Order;
using CommerceCore.Application.Abstractions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Commerce.Modules.Order.Tests.TestCommon;

internal sealed class MutableDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow { get; set; } = DateTime.UtcNow;
}

internal static class OrderTestFixture
{
    public static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<OrderMappingProfile>(), NullLoggerFactory.Instance);
        return config.CreateMapper();
    }

    public static OrderDbContext CreateInMemoryDbContext(
        IDateTimeProvider? dateTimeProvider = null,
        ICurrentUser? currentUser = null,
        string? databaseName = null)
    {
        var connectionName = databaseName ?? Guid.NewGuid().ToString("N");
        var connection = new SqliteConnection($"Data Source={connectionName};Mode=Memory;Cache=Shared");
        connection.Open();

        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseSqlite(connection);

        if (string.Equals(Environment.GetEnvironmentVariable("ORDER_TEST_LOG_SQL"), "1", StringComparison.Ordinal))
        {
            options
                .EnableSensitiveDataLogging()
                .LogTo(Console.WriteLine, LogLevel.Information);
        }

        var dbContext = new OrderDbContext(
            options.Options,
            dateTimeProvider ?? CreateDateTimeProvider(),
            currentUser ?? CreateCurrentUser());

        dbContext.Database.EnsureCreated();
        return dbContext;
    }

    public static SqliteConnection CreateSharedInMemoryConnection(string? databaseName = null)
    {
        var connectionName = databaseName ?? Guid.NewGuid().ToString("N");
        var connection = new SqliteConnection($"Data Source={connectionName};Mode=Memory;Cache=Shared");
        connection.Open();
        return connection;
    }

    public static OrderDbContext CreateInMemoryDbContext(
        SqliteConnection connection,
        IDateTimeProvider? dateTimeProvider = null,
        ICurrentUser? currentUser = null,
        bool ensureCreated = false)
    {
        ArgumentNullException.ThrowIfNull(connection);

        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseSqlite(connection);

        if (string.Equals(Environment.GetEnvironmentVariable("ORDER_TEST_LOG_SQL"), "1", StringComparison.Ordinal))
        {
            options
                .EnableSensitiveDataLogging()
                .LogTo(Console.WriteLine, LogLevel.Information);
        }

        var dbContext = new OrderDbContext(
            options.Options,
            dateTimeProvider ?? CreateDateTimeProvider(),
            currentUser ?? CreateCurrentUser());

        if (ensureCreated)
        {
            dbContext.Database.EnsureCreated();
        }

        return dbContext;
    }

    public static IDateTimeProvider CreateDateTimeProvider(DateTime? utcNow = null) =>
        new MutableDateTimeProvider { UtcNow = utcNow ?? DateTime.UtcNow };

    public static MutableDateTimeProvider CreateMutableDateTimeProvider(DateTime? utcNow = null) =>
        new() { UtcNow = utcNow ?? DateTime.UtcNow };

    public static OrderDbContext CreateInMemoryDbContext(
        MutableDateTimeProvider dateTimeProvider,
        ICurrentUser? currentUser = null,
        string? databaseName = null)
    {
        return CreateInMemoryDbContext(
            (IDateTimeProvider)dateTimeProvider,
            currentUser,
            databaseName);
    }

    public static void AdvanceTime(MutableDateTimeProvider provider, TimeSpan delta)
    {
        provider.UtcNow = provider.UtcNow.Add(delta);
    }

    public static void SetTime(MutableDateTimeProvider provider, DateTime utcNow)
    {
        provider.UtcNow = utcNow;
    }

    public static void EnsureIncreasingTime(MutableDateTimeProvider provider)
    {
        provider.UtcNow = provider.UtcNow.AddMilliseconds(1);
    }

    public static ICurrentUser CreateCurrentUser(string? userId = null, string? userName = null)
    {
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns(userId ?? Guid.NewGuid().ToString());
        currentUser.UserName.Returns(userName ?? "test-user");
        currentUser.IsAuthenticated.Returns(true);
        return currentUser;
    }

    public static CreateOrderCommand CreateValidCreateOrderCommand() =>
        new()
        {
            CustomerEmail = "test@example.com",
            Currency = "USD",
            ShippingAddress = new CreateOrderAddressRequest(
                FullName: "John Doe",
                PhoneNumber: "1234567890",
                AddressLine1: "123 Main St",
                AddressLine2: null,
                City: "New York",
                State: "NY",
                PostalCode: "10001",
                Country: "USA"),
            BillingAddress = new CreateOrderAddressRequest(
                FullName: "John Doe",
                PhoneNumber: "1234567890",
                AddressLine1: "123 Main St",
                AddressLine2: null,
                City: "New York",
                State: "NY",
                PostalCode: "10001",
                Country: "USA"),
            Items =
            [
                new CreateOrderItemRequest(
                    ProductId: Guid.NewGuid(),
                    ProductName: "Test Product",
                    ProductSku: "TEST-SKU",
                    VariantId: null,
                    VariantName: null,
                    UnitPrice: 100m,
                    Quantity: 2,
                    DiscountAmount: 10m)
            ],
            DiscountAmount = 10m,
            ShippingAmount = 5m,
            TaxAmount = 15m
        };

    public static OrderEntity CreateOrderEntity(
        OrderStatus status = OrderStatus.Pending,
        Guid? customerId = null,
        string orderNumber = "ORD-TEST",
        string customerEmail = "test@example.com",
        DateTime? createdAtUtc = null,
        bool isDeleted = false,
        decimal totalAmount = 0m,
        params OrderItem[] items)
    {
        return new OrderEntity
        {
            Id = Guid.NewGuid(),
            OrderNumber = orderNumber,
            CustomerId = customerId,
            CustomerEmail = customerEmail,
            Status = status,
            TotalAmount = totalAmount,
            Currency = "USD",
            CreatedAtUtc = createdAtUtc ?? DateTime.UtcNow,
            IsDeleted = isDeleted,
            DeletedAtUtc = isDeleted ? DateTime.UtcNow : null,
            ShippingAddress = new OrderAddress
            {
                FullName = "Test",
                PhoneNumber = "123",
                AddressLine1 = "Test",
                City = "Test",
                Country = "Test"
            },
            BillingAddress = new OrderAddress
            {
                FullName = "Test",
                PhoneNumber = "123",
                AddressLine1 = "Test",
                City = "Test",
                Country = "Test"
            },
            Items = items.ToList()
        };
    }

    public static OrderItem CreateOrderItem(Guid? variantId = null, int quantity = 1) =>
        new()
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.Empty,
            ProductId = Guid.NewGuid(),
            ProductName = "Product",
            ProductSku = "SKU-001",
            VariantId = variantId,
            VariantName = variantId.HasValue ? "Variant" : null,
            UnitPrice = 10m,
            Quantity = quantity,
            DiscountAmount = 0m,
            TotalAmount = quantity * 10m
        };

    public static InventoryAvailabilityResponse CreateInventoryAvailabilityResponse(Guid orderId, params InventoryAvailabilityItemResponse[] items) =>
        new(Guid.NewGuid(), orderId, "Active", DateTime.UtcNow, null, items);

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
}
