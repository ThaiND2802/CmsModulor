using System.Reflection;

namespace Commerce.Modules.Inventory.Contracts;

public static class InventoryPermissions
{
    public const string StockView = "inventory:stock:view";
    public const string StockReceive = "inventory:stock:receive";
    public const string StockAdjust = "inventory:stock:adjust";
    public const string StockViewMovements = "inventory:stock:view-movements";
    public const string ReservationsCreate = "inventory:reservations:create";
    public const string ReservationsRelease = "inventory:reservations:release";

    public static IEnumerable<string> All() =>
        typeof(InventoryPermissions)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(static field => field.IsLiteral && !field.IsInitOnly)
            .Select(static field => (string)field.GetValue(null)!);
}
