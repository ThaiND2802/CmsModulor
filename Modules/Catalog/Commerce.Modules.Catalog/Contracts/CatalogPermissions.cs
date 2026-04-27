using System.Reflection;

namespace Commerce.Modules.Catalog.Contracts;

public static class CatalogPermissions
{
    public const string CategoriesView = "catalog:categories:view";
    public const string CategoriesCreate = "catalog:categories:create";
    public const string CategoriesEdit = "catalog:categories:edit";
    public const string CategoriesDelete = "catalog:categories:delete";

    public const string BrandsView = "catalog:brands:view";
    public const string BrandsCreate = "catalog:brands:create";
    public const string BrandsEdit = "catalog:brands:edit";
    public const string BrandsDelete = "catalog:brands:delete";

    public const string ProductsView = "catalog:products:view";
    public const string ProductsCreate = "catalog:products:create";
    public const string ProductsEdit = "catalog:products:edit";
    public const string ProductsDelete = "catalog:products:delete";

    public static IEnumerable<string> All() =>
        typeof(CatalogPermissions)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(static field => field.IsLiteral && !field.IsInitOnly)
            .Select(static field => (string)field.GetValue(null)!);
}
