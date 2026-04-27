using Commerce.Modules.Identity.Domain;
using Commerce.Modules.Identity.Infrastructure;
using Commerce.Modules.Sale.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Host.WebApi.Extensions;

public static class SalePermissionSeederExtensions
{
    public static async Task SeedSalePermissionsAsync(this IServiceProvider serviceProvider, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        using var scope = serviceProvider.CreateScope();
        var identityDb = scope.ServiceProvider.GetService<IdentityDbContext>();
        if (identityDb is null)
        {
            return;
        }

        var salePermissionCodes = SalePermissions.All().ToArray();

        var existingCodes = await identityDb.Permissions
            .IgnoreQueryFilters()
            .Where(permission => permission.Module == "Sale")
            .Select(permission => permission.Code)
            .ToListAsync(ct);

        var existingCodeSet = new HashSet<string>(existingCodes, StringComparer.OrdinalIgnoreCase);

        var permissionsToAdd = salePermissionCodes
            .Where(code => !existingCodeSet.Contains(code))
            .Select((code, index) => new Permission
            {
                Id = Guid.NewGuid(),
                Code = code,
                Name = code,
                Description = $"Allows {code.Replace("sale:sales:", string.Empty).Replace('-', ' ')} operations.",
                Module = "Sale",
                Feature = "Sales",
                GroupName = "Sale",
                SortOrder = 100 + index,
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
}
