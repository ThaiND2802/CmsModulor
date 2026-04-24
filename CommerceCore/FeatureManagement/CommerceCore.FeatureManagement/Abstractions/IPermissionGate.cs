namespace CommerceCore.FeatureManagement.Abstractions;

public interface IPermissionGate
{
    bool HasPermission(string userId, string permission)
    {
        throw new NotSupportedException("Synchronous permission checks are not supported. Use HasPermissionAsync instead.");
    }

    Task<bool> HasPermissionAsync(string userId, string permission, CancellationToken cancellationToken = default);
}
