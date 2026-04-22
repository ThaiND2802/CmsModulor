using CommerceCore.FeatureManagement.Abstractions;

namespace CommerceCore.FeatureManagement.Providers;

public sealed class DenyAllPermissionGate : IPermissionGate
{
    public bool HasPermission(string userId, string permission)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(permission);

        return false;
    }

    public Task<bool> HasPermissionAsync(string userId, string permission, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(permission);

        return Task.FromResult(false);
    }
}
