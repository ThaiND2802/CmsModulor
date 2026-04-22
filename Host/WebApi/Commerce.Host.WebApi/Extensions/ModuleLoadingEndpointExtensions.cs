using CommerceCore.FeatureManagement.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Commerce.Host.WebApi.Extensions;

public static class ModuleLoadingEndpointExtensions
{
    public static IEndpointRouteBuilder MapModuleLoadingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapGet("/api/modules", (IModuleGate moduleGate) =>
        {
            var modules = moduleGate.GetAll()
                .OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
                .Select(pair => new
                {
                    module = pair.Key,
                    enabled = pair.Value
                });

            return Results.Ok(modules);
        })
        .WithTags("Modules");

        return endpoints;
    }
}
