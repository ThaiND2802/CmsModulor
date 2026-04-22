using Commerce.Host.WebApi.Configuration;
using CommerceCore.FeatureManagement.Abstractions;
using CommerceCore.FeatureManagement.Attributes;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using System.Net.Http;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Commerce.Host.WebApi.OpenApi;

public sealed class FeatureAwareSwaggerDocumentFilter : IDocumentFilter
{
    private readonly IModuleGate _moduleGate;
    private readonly IFeatureGate _featureGate;
    private readonly IOptions<OpenApiVisibilityOptions> _options;

    public FeatureAwareSwaggerDocumentFilter(
        IModuleGate moduleGate,
        IFeatureGate featureGate,
        IOptions<OpenApiVisibilityOptions> options)
    {
        _moduleGate = moduleGate;
        _featureGate = featureGate;
        _options = options;
    }

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        if (!_options.Value.HideDisabledEndpoints)
        {
            return;
        }

        foreach (var apiDescription in context.ApiDescriptions)
        {
            var metadata = apiDescription.ActionDescriptor.EndpointMetadata;
            var disabledByModule = metadata.OfType<RequireModuleAttribute>().Any(x => !_moduleGate.IsEnabled(x.ModuleName));
            var disabledByFeature = metadata.OfType<RequireFeatureAttribute>().Any(x => !_featureGate.IsEnabledAsync(x.FeatureName).AsTask().GetAwaiter().GetResult());
            if (!disabledByModule && !disabledByFeature)
            {
                continue;
            }

            var pathKey = ToOpenApiPath(apiDescription.RelativePath);
            if (!swaggerDoc.Paths.TryGetValue(pathKey, out var pathItem))
            {
                continue;
            }

            var operationType = ToOperationType(apiDescription.HttpMethod);
            if (operationType is null)
            {
                continue;
            }

            if (pathItem.Operations is null)
            {
                continue;
            }

            pathItem.Operations.Remove(operationType);
            if (pathItem.Operations.Count == 0)
            {
                swaggerDoc.Paths.Remove(pathKey);
            }
        }
    }

    private static string ToOpenApiPath(string? relativePath)
    {
        var path = relativePath ?? string.Empty;
        var queryIndex = path.IndexOf('?', StringComparison.Ordinal);
        if (queryIndex >= 0)
        {
            path = path[..queryIndex];
        }

        path = path.TrimStart('/');
        return $"/{path}";
    }

    private static HttpMethod? ToOperationType(string? httpMethod)
    {
        return httpMethod?.ToUpperInvariant() switch
        {
            "GET" => HttpMethod.Get,
            "POST" => HttpMethod.Post,
            "PUT" => HttpMethod.Put,
            "DELETE" => HttpMethod.Delete,
            "PATCH" => HttpMethod.Patch,
            "HEAD" => HttpMethod.Head,
            "OPTIONS" => HttpMethod.Options,
            "TRACE" => HttpMethod.Trace,
            _ => null
        };
    }
}
