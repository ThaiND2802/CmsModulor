using Commerce.Modules.Order.Infrastructure;
using Commerce.Modules.Order.Tests.TestCommon;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Commerce.Modules.Order.Tests.Integration;

public sealed class OrderIntegrationFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("cms_order_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public string ConnectionString => _dbContainer.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();
    }

    public OrderDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new OrderDbContext(
            options,
            OrderTestFixture.CreateDateTimeProvider(),
            OrderTestFixture.CreateCurrentUser());
    }

    public async Task ResetAsync()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE order_orders CASCADE;");
    }

    public async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
    }
}
