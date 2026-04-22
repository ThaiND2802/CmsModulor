using CommerceCore.FeatureManagement.Abstractions;
using CommerceCore.FeatureManagement.Models;
using Microsoft.Extensions.Options;

namespace CommerceCore.FeatureManagement.Providers;

public sealed class InMemoryPermissionGate : IPermissionGate
{
    private readonly IOptionsMonitor<PermissionOptions> _optionsMonitor;

    public InMemoryPermissionGate(IOptionsMonitor<PermissionOptions> optionsMonitor)
    {
        _optionsMonitor = optionsMonitor;
    }

    public bool HasPermission(string userId, string permission)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(permission);

        var users = _optionsMonitor.CurrentValue.Users;
        if (!users.TryGetValue(userId, out var permissions))
        {
            return false;
        }

        return permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }

    public Task<bool> HasPermissionAsync(string userId, string permission, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(HasPermission(userId, permission));
    }
}
