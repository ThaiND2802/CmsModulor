using CommerceCore.FeatureManagement.Abstractions;
using CommerceCore.FeatureManagement.Attributes;
using CommerceCore.SharedKernel.Exceptions;
using Microsoft.AspNetCore.Http;

namespace CommerceCore.FeatureManagement.Middleware;

public sealed class FeatureGateMiddleware
{
    private readonly RequestDelegate _next;

    public FeatureGateMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IModuleGate moduleGate,
        IFeatureGate featureGate)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(moduleGate);
        ArgumentNullException.ThrowIfNull(featureGate);

        var endpoint = context.GetEndpoint();
        if (endpoint is null)
        {
            await _next(context);
            return;
        }

        var requiredModules = endpoint.Metadata.GetOrderedMetadata<RequireModuleAttribute>();
        foreach (var requirement in requiredModules)
        {
            if (moduleGate.IsEnabled(requirement.ModuleName))
            {
                continue;
            }

            throw new NotFoundAppException($"Module '{requirement.ModuleName}' is disabled.");
        }

        var requiredFeatures = endpoint.Metadata.GetOrderedMetadata<RequireFeatureAttribute>();
        foreach (var requirement in requiredFeatures)
        {
            var isEnabled = await featureGate.IsEnabledAsync(requirement.FeatureName, context.RequestAborted);
            if (isEnabled)
            {
                continue;
            }

            throw new NotFoundAppException($"Feature '{requirement.FeatureName}' is not enabled.");
        }

        await _next(context);
    }
}
