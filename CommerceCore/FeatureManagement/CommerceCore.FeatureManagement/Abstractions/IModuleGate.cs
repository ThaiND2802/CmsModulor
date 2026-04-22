namespace CommerceCore.FeatureManagement.Abstractions;

public interface IModuleGate
{
    bool IsEnabled(string moduleName);

    Task<bool> IsEnabledAsync(string moduleName, CancellationToken cancellationToken = default);

    IReadOnlyDictionary<string, bool> GetAll();
}
