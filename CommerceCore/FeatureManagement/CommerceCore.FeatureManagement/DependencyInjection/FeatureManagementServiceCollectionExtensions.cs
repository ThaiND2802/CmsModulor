using CommerceCore.FeatureManagement.Abstractions;
using CommerceCore.FeatureManagement.Models;
using CommerceCore.FeatureManagement.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CommerceCore.FeatureManagement.DependencyInjection;

public static class FeatureManagementServiceCollectionExtensions
{
    public static IServiceCollection AddCommerceFeatureManagement(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<ModuleOptions>(configuration.GetSection(ModuleOptions.SectionName));
        services.Configure<FeatureOptions>(configuration.GetSection(FeatureOptions.SectionName));
        services.Configure<PermissionOptions>(configuration.GetSection(PermissionOptions.SectionName));
        services.AddSingleton<IModuleGate, InMemoryModuleGate>();
        services.AddSingleton<IFeatureGate, InMemoryFeatureGate>();
        services.AddSingleton<IPermissionGate, DenyAllPermissionGate>();

        return services;
    }
}
