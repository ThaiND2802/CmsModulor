using Commerce.Modules.Catalog.Infrastructure;
using CommerceCore.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Testcontainers.PostgreSql;

namespace Commerce.Modules.Catalog.Tests.Common;

public sealed class PostgresCatalogFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await ResetAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    public async Task ResetAsync()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.MigrateAsync();
    }

    public CatalogDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        var dateTimeProvider = Substitute.For<IDateTimeProvider>();
        dateTimeProvider.UtcNow.Returns(new DateTime(2026, 4, 24, 0, 0, 0, DateTimeKind.Utc));

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns("integration-user");
        currentUser.UserName.Returns("integration-tester");
        currentUser.IsAuthenticated.Returns(true);

        return new CatalogDbContext(options, dateTimeProvider, currentUser);
    }
}
