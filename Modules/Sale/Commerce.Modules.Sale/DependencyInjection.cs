using CommerceCore.FeatureManagement.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Commerce.Modules.Sale;

public static class DependencyInjection
{
    public static IServiceCollection AddSaleModule(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        return services;
    }

    public static IEndpointRouteBuilder MapSaleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        return endpoints.MapCommerceModule("Sale", "/api/sale", "Sale", group =>
        {
            group.MapGet("/health", () => Results.Ok(new
            {
                module = "Sale",
                status = "Healthy"
            }));
        });
    }
}
