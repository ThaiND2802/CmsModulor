namespace CommerceCore.FeatureManagement.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class RequireFeatureAttribute : Attribute
{
    public RequireFeatureAttribute(string featureName)
    {
        FeatureName = string.IsNullOrWhiteSpace(featureName)
            ? throw new ArgumentException("Feature name is required.", nameof(featureName))
            : featureName;
    }

    public string FeatureName { get; }
}
