using System.Reflection;

namespace Commerce.Modules.Order.Contracts;

public static class OrderPermissions
{
    public const string OrdersView = "order:orders:view";
    public const string OrdersCreate = "order:orders:create";
    public const string OrdersConfirm = "order:orders:confirm";
    public const string OrdersUpdateStatus = "order:orders:update-status";
    public const string OrdersCancel = "order:orders:cancel";
    public const string OrdersUpdateNotes = "order:orders:update-notes";
    public const string OrdersViewOwn = "order:orders:view-own";

    public static IEnumerable<string> All() =>
        typeof(OrderPermissions)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(static field => field.IsLiteral && !field.IsInitOnly)
            .Select(static field => (string)field.GetValue(null)!);
}
