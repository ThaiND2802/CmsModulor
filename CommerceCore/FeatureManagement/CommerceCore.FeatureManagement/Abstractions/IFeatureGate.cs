namespace CommerceCore.FeatureManagement.Abstractions;

public interface IFeatureGate
{
    ValueTask<bool> IsEnabledAsync(string featureName, CancellationToken cancellationToken = default);
}
