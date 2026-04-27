using System.Reflection;

namespace Commerce.Modules.Sale.Contracts;

public static class SalePermissions
{
    public const string SalesView = "sale:sales:view";
    public const string SalesCreate = "sale:sales:create";
    public const string SalesEdit = "sale:sales:edit";
    public const string SalesSubmit = "sale:sales:submit";
    public const string SalesCancel = "sale:sales:cancel";
    public const string SalesOverridePrice = "sale:sales:override-price";

    public static IEnumerable<string> All() =>
        typeof(SalePermissions)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(static field => field.IsLiteral && !field.IsInitOnly)
            .Select(static field => (string)field.GetValue(null)!);
}
