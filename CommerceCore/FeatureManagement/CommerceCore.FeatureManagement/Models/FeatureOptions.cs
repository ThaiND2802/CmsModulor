namespace CommerceCore.FeatureManagement.Models;

public sealed class FeatureOptions
{
    public const string SectionName = "Features";

    public Dictionary<string, bool> Flags { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
