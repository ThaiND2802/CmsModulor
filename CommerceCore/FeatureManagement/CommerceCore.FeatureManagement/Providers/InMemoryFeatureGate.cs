using CommerceCore.FeatureManagement.Abstractions;
using CommerceCore.FeatureManagement.Models;
using Microsoft.Extensions.Options;

namespace CommerceCore.FeatureManagement.Providers;

public sealed class InMemoryFeatureGate : IFeatureGate
{
    private readonly IOptionsMonitor<FeatureOptions> _optionsMonitor;

    public InMemoryFeatureGate(IOptionsMonitor<FeatureOptions> optionsMonitor)
    {
        _optionsMonitor = optionsMonitor;
    }

    public ValueTask<bool> IsEnabledAsync(string featureName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(featureName);

        var flags = _optionsMonitor.CurrentValue.Flags;
        var isEnabled = flags.TryGetValue(featureName, out var enabled) && enabled;

        return ValueTask.FromResult(isEnabled);
    }
}
