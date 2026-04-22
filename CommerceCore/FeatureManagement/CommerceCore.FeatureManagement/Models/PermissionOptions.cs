namespace CommerceCore.FeatureManagement.Models;

public sealed class PermissionOptions
{
    public const string SectionName = "Permissions";

    public Dictionary<string, string[]> Users { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
