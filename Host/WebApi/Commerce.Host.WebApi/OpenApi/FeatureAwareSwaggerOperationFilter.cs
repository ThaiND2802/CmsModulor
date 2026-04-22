using Commerce.Host.WebApi.Configuration;
using CommerceCore.FeatureManagement.Abstractions;
using CommerceCore.FeatureManagement.Attributes;
using CommerceCore.FeatureManagement.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using System.Text.Json;
using System.Text.Json.Nodes;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Commerce.Host.WebApi.OpenApi;

public sealed class FeatureAwareSwaggerOperationFilter : IOperationFilter
{
    private readonly IModuleGate _moduleGate;
    private readonly IFeatureGate _featureGate;
    private readonly IOptions<OpenApiVisibilityOptions> _options;

    public FeatureAwareSwaggerOperationFilter(
        IModuleGate moduleGate,
        IFeatureGate featureGate,
        IOptions<OpenApiVisibilityOptions> options)
    {
        _moduleGate = moduleGate;
        _featureGate = featureGate;
        _options = options;
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;
        var requiredModules = metadata.OfType<RequireModuleAttribute>().Select(x => x.ModuleName).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var requiredFeatures = metadata.OfType<RequireFeatureAttribute>().Select(x => x.FeatureName).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var metadataPermissions = metadata.OfType<RequirePermissionAttribute>().Select(x => x.Permission);
        var policyPermissions = metadata.OfType<AuthorizeAttribute>()
            .Select(x => x.Policy)
            .Where(static x => !string.IsNullOrWhiteSpace(x))
            .Select(static x => x!)
            .Where(x => x.StartsWith($"{PermissionAuthorizeAttribute.PolicyPrefix}:", StringComparison.Ordinal))
            .Select(x => x.Substring($"{PermissionAuthorizeAttribute.PolicyPrefix}:".Length));
        var requiredPermissions = metadataPermissions
            .Concat(policyPermissions)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (requiredModules.Length > 0)
        {
            operation.Extensions ??= new Dictionary<string, IOpenApiExtension>(StringComparer.Ordinal);
            operation.Extensions["x-commerce-required-modules"] = new JsonNodeExtension(JsonSerializer.SerializeToNode(requiredModules)!);
        }

        if (requiredFeatures.Length > 0)
        {
            operation.Extensions ??= new Dictionary<string, IOpenApiExtension>(StringComparer.Ordinal);
            operation.Extensions["x-commerce-required-features"] = new JsonNodeExtension(JsonSerializer.SerializeToNode(requiredFeatures)!);
        }

        if (requiredPermissions.Length > 0)
        {
            operation.Extensions ??= new Dictionary<string, IOpenApiExtension>(StringComparer.Ordinal);
            operation.Extensions["x-commerce-required-permissions"] = new JsonNodeExtension(JsonSerializer.SerializeToNode(requiredPermissions)!);
        }

        if (!_options.Value.MarkDisabledEndpoints || (requiredModules.Length == 0 && requiredFeatures.Length == 0 && requiredPermissions.Length == 0))
        {
            return;
        }

        var availability = new List<string>();
        foreach (var moduleName in requiredModules)
        {
            availability.Add($"Module `{moduleName}`: {(_moduleGate.IsEnabled(moduleName) ? "enabled" : "disabled")}");
        }

        foreach (var featureName in requiredFeatures)
        {
            var isEnabled = _featureGate.IsEnabledAsync(featureName).AsTask().GetAwaiter().GetResult();
            availability.Add($"Feature `{featureName}`: {(isEnabled ? "enabled" : "disabled")}");
        }

        foreach (var permission in requiredPermissions)
        {
            availability.Add($"Permission `{permission}`: required");
        }

        if (availability.Count == 0)
        {
            return;
        }

        var note = $"Availability: {string.Join("; ", availability)}.";
        operation.Description = string.IsNullOrWhiteSpace(operation.Description)
            ? note
            : $"{operation.Description.Trim()}\n\n{note}";
    }
}
