using CommerceCore.FeatureManagement.Abstractions;

namespace CommerceCore.FeatureManagement.Providers;

public sealed class DenyAllPermissionGate : IPermissionGate
{
    public Task<bool> HasPermissionAsync(string userId, string permission, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(permission);

        return Task.FromResult(false);
    }
}
