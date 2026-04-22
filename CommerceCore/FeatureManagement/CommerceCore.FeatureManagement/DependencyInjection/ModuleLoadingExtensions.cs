using CommerceCore.FeatureManagement.Abstractions;
using CommerceCore.FeatureManagement.Models;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CommerceCore.FeatureManagement.DependencyInjection;

public static class ModuleLoadingExtensions
{
    public static IServiceCollection AddModuleIfEnabled(
        this IServiceCollection services,
        IConfiguration configuration,
        string moduleName,
        Func<IServiceCollection, IConfiguration, IServiceCollection> registerModule)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleName);
        ArgumentNullException.ThrowIfNull(registerModule);

        var isEnabled = configuration.GetValue<bool>($"{ModuleOptions.SectionName}:Enabled:{moduleName}");

        if (!isEnabled)
        {
            LogModuleSkip(services, moduleName, "registration");
            return services;
        }

        return registerModule(services, configuration);
    }

    public static IEndpointRouteBuilder MapModuleIfEnabled(
        this IEndpointRouteBuilder endpoints,
        string moduleName,
        Func<IEndpointRouteBuilder, IEndpointRouteBuilder> mapModule)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleName);
        ArgumentNullException.ThrowIfNull(mapModule);

        var moduleGate = endpoints.ServiceProvider.GetRequiredService<IModuleGate>();
        if (!moduleGate.IsEnabled(moduleName))
        {
            LogModuleSkip(endpoints.ServiceProvider, moduleName, "endpoint mapping");
            return endpoints;
        }

        return mapModule(endpoints);
    }

    private static void LogModuleSkip(IServiceCollection services, string moduleName, string action)
    {
        using var provider = services.BuildServiceProvider();
        LogModuleSkip(provider, moduleName, action);
    }

    private static void LogModuleSkip(IServiceProvider serviceProvider, string moduleName, string action)
    {
        var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
        var logger = loggerFactory?.CreateLogger("CommerceCore.FeatureManagement.ModuleLoading");
        logger?.LogInformation("Module {ModuleName} is disabled. Skipping {Action}.", moduleName, action);
    }
}
