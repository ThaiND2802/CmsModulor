using Commerce.Modules.Identity.Domain;
using Commerce.Modules.Identity.Infrastructure;
using Commerce.Modules.Order.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Host.WebApi.Extensions;

public static class OrderPermissionSeederExtensions
{
    public static async Task SeedOrderPermissionsAsync(this IServiceProvider serviceProvider, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        using var scope = serviceProvider.CreateScope();
        var identityDb = scope.ServiceProvider.GetService<IdentityDbContext>();
        if (identityDb is null)
        {
            return;
        }

        var orderPermissionCodes = OrderPermissions.All().ToArray();

        var existingCodes = await identityDb.Permissions
            .IgnoreQueryFilters()
            .Where(permission => permission.Module == "Order")
            .Select(permission => permission.Code)
            .ToListAsync(ct);

        var existingCodeSet = new HashSet<string>(existingCodes, StringComparer.OrdinalIgnoreCase);

        var permissionsToAdd = orderPermissionCodes
            .Where(code => !existingCodeSet.Contains(code))
            .Select((code, index) => new Permission
            {
                Id = Guid.NewGuid(),
                Code = code,
                Name = code,
                Description = $"Allows {code.Replace("order:orders:", string.Empty).Replace('-', ' ')} operations.",
                Module = "Order",
                Feature = "Orders",
                GroupName = "Order",
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
