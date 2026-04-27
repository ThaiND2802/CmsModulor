using Commerce.Host.WebApi.Extensions;
using Commerce.Modules.Inventory.Contracts;
using Commerce.Modules.Inventory.Tests.TestCommon;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Commerce.Modules.Inventory.Tests.Infrastructure;

public sealed class InventoryPermissionSeederExtensionsTests
{
    [Fact]
    public async Task SeedInventoryPermissionsAsync_IsIdempotent()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        var services = new ServiceCollection();
        services.AddScoped(_ => InventoryTestFixture.CreateIdentityDbContext(databaseName));
        await using var serviceProvider = services.BuildServiceProvider();

        await serviceProvider.SeedInventoryPermissionsAsync();
        await serviceProvider.SeedInventoryPermissionsAsync();

        await using var assertionContext = InventoryTestFixture.CreateIdentityDbContext(databaseName);
        var permissions = await assertionContext.Permissions
            .IgnoreQueryFilters()
            .Where(x => x.Module == "Inventory")
            .OrderBy(x => x.Code)
            .ToListAsync();

        permissions.Select(x => x.Code).Should().Equal(InventoryPermissions.All().OrderBy(static x => x));
        permissions.Should().OnlyHaveUniqueItems(x => x.Code);
    }
}
