using Commerce.Host.WebApi.Extensions;
using Commerce.Modules.Sale.Contracts;
using Commerce.Modules.Sale.Tests.TestCommon;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Commerce.Modules.Sale.Tests.Infrastructure;

public sealed class SalePermissionSeederExtensionsTests
{
    [Fact]
    public async Task SeedSalePermissionsAsync_IsIdempotent()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        var services = new ServiceCollection();
        services.AddScoped(_ => SaleTestFixture.CreateIdentityDbContext(databaseName));
        await using var serviceProvider = services.BuildServiceProvider();

        await serviceProvider.SeedSalePermissionsAsync();
        await serviceProvider.SeedSalePermissionsAsync();

        await using var assertionContext = SaleTestFixture.CreateIdentityDbContext(databaseName);
        var permissions = await assertionContext.Permissions
            .IgnoreQueryFilters()
            .Where(x => x.Module == "Sale")
            .OrderBy(x => x.Code)
            .ToListAsync();

        permissions.Select(x => x.Code).Should().Equal(SalePermissions.All().OrderBy(static x => x));
        permissions.Should().OnlyHaveUniqueItems(x => x.Code);
    }
}
