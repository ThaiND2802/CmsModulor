using CommerceCore.FeatureManagement.Abstractions;
using CommerceCore.FeatureManagement.Attributes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CommerceCore.FeatureManagement.DependencyInjection;

public static class CommerceModuleEndpointExtensions
{
    public static IEndpointRouteBuilder MapCommerceModule(
        this IEndpointRouteBuilder endpoints,
        string moduleName,
        string routePrefix,
        string tag,
        Action<RouteGroupBuilder> mapEndpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleName);
        ArgumentException.ThrowIfNullOrWhiteSpace(routePrefix);
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);
        ArgumentNullException.ThrowIfNull(mapEndpoints);

        var moduleGate = endpoints.ServiceProvider.GetRequiredService<IModuleGate>();
        if (!moduleGate.IsEnabled(moduleName))
        {
            LogModuleSkip(endpoints.ServiceProvider, moduleName, routePrefix);
            return endpoints;
        }

        var group = endpoints.MapGroup(routePrefix);
        group.WithTags(tag);
        group.RequireModule(moduleName);

        mapEndpoints(group);

        return endpoints;
    }

    public static TBuilder RequireModule<TBuilder>(this TBuilder builder, string moduleName)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleName);

        builder.WithMetadata(new RequireModuleAttribute(moduleName));
        return builder;
    }

    public static TBuilder RequireFeature<TBuilder>(this TBuilder builder, string featureName)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(featureName);

        builder.WithMetadata(new RequireFeatureAttribute(featureName));
        return builder;
    }

    private static void LogModuleSkip(IServiceProvider serviceProvider, string moduleName, string routePrefix)
    {
        var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
        var logger = loggerFactory?.CreateLogger("CommerceCore.FeatureManagement.ModuleEndpoints");
        logger?.LogInformation(
            "Module {ModuleName} is disabled. Skipping route group {RoutePrefix}.",
            moduleName,
            routePrefix);
    }
}
