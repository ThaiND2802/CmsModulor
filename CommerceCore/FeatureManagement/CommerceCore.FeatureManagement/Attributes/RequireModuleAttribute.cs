namespace CommerceCore.FeatureManagement.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class RequireModuleAttribute : Attribute
{
    public RequireModuleAttribute(string moduleName)
    {
        ModuleName = string.IsNullOrWhiteSpace(moduleName)
            ? throw new ArgumentException("Module name is required.", nameof(moduleName))
            : moduleName;
    }

    public string ModuleName { get; }
}
