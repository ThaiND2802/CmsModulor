namespace CommerceCore.FeatureManagement.Models;

public sealed class ModuleOptions
{
    public const string SectionName = "Modules";

    public Dictionary<string, bool> Enabled { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
