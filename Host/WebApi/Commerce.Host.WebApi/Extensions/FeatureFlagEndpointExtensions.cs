using CommerceCore.FeatureManagement.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Commerce.Host.WebApi.Extensions;

public static class FeatureFlagEndpointExtensions
{
    public static IEndpointRouteBuilder MapFeatureFlagEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapGet("/api/features/{featureName}", async (string featureName, IFeatureGate featureGate, CancellationToken cancellationToken) =>
        {
            var isEnabled = await featureGate.IsEnabledAsync(featureName, cancellationToken);

            return Results.Ok(new
            {
                feature = featureName,
                enabled = isEnabled
            });
        })
        .WithTags("Features");

        return endpoints;
    }
}
