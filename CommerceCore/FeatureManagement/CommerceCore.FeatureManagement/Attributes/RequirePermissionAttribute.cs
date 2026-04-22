namespace CommerceCore.FeatureManagement.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class RequirePermissionAttribute : Attribute
{
    public RequirePermissionAttribute(string permission)
    {
        Permission = string.IsNullOrWhiteSpace(permission)
            ? throw new ArgumentException("Permission is required.", nameof(permission))
            : permission;
    }

    public string Permission { get; }
}
