using Commerce.Modules.Identity.Domain;
using Commerce.Modules.Identity.Infrastructure;
using Commerce.Modules.Inventory.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Host.WebApi.Extensions;

public static class InventoryPermissionSeederExtensions
{
    public static async Task SeedInventoryPermissionsAsync(this IServiceProvider serviceProvider, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        using var scope = serviceProvider.CreateScope();
        var identityDb = scope.ServiceProvider.GetService<IdentityDbContext>();
        if (identityDb is null)
        {
            return;
        }

        var inventoryPermissionCodes = InventoryPermissions.All().ToArray();

        var existingCodes = await identityDb.Permissions
            .IgnoreQueryFilters()
            .Where(permission => permission.Module == "Inventory")
            .Select(permission => permission.Code)
            .ToListAsync(ct);

        var existingCodeSet = new HashSet<string>(existingCodes, StringComparer.OrdinalIgnoreCase);

        var permissionsToAdd = inventoryPermissionCodes
            .Where(code => !existingCodeSet.Contains(code))
            .Select((code, index) => new Permission
            {
                Id = Guid.NewGuid(),
                Code = code,
                Name = code,
                Description = $"Allows {code.Replace("inventory:", string.Empty).Replace(':', ' ').Replace('-', ' ')} operations.",
                Module = "Inventory",
                Feature = GetFeature(code),
                GroupName = "Inventory",
                SortOrder = 200 + index,
                IsActive = true,
                IsSystem = false
            })
            .ToList();

        if (permissionsToAdd.Count == 0)
        {
            return;
        }

        await identityDb.Permissions.AddRangeAsync(permissionsToAdd, ct);
        await identityDb.SaveChangesAsync(ct);
    }

    private static string GetFeature(string code) =>
        code.StartsWith("inventory:reservations:", StringComparison.OrdinalIgnoreCase)
            ? "Reservations"
            : "Stock";
}
