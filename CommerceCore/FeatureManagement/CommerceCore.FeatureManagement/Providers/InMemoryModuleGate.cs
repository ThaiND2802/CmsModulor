using CommerceCore.FeatureManagement.Abstractions;
using CommerceCore.FeatureManagement.Models;
using Microsoft.Extensions.Options;

namespace CommerceCore.FeatureManagement.Providers;

public sealed class InMemoryModuleGate : IModuleGate
{
    private readonly IOptionsMonitor<ModuleOptions> _optionsMonitor;

    public InMemoryModuleGate(IOptionsMonitor<ModuleOptions> optionsMonitor)
    {
        _optionsMonitor = optionsMonitor;
    }

    public bool IsEnabled(string moduleName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleName);

        return _optionsMonitor.CurrentValue.Enabled.TryGetValue(moduleName, out var enabled) && enabled;
    }

    public Task<bool> IsEnabledAsync(string moduleName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(IsEnabled(moduleName));
    }

    public IReadOnlyDictionary<string, bool> GetAll()
    {
        return new Dictionary<string, bool>(_optionsMonitor.CurrentValue.Enabled, StringComparer.OrdinalIgnoreCase);
    }
}
