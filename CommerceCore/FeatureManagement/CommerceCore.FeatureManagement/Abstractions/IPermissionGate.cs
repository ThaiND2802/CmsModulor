namespace CommerceCore.FeatureManagement.Abstractions;

public interface IPermissionGate
{
    bool HasPermission(string userId, string permission);

    Task<bool> HasPermissionAsync(string userId, string permission, CancellationToken cancellationToken = default);
}
