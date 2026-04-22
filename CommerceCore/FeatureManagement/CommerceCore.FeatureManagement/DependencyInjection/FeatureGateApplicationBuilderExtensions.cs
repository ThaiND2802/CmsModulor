using CommerceCore.FeatureManagement.Middleware;
using Microsoft.AspNetCore.Builder;

namespace CommerceCore.FeatureManagement.DependencyInjection;

public static class FeatureGateApplicationBuilderExtensions
{
    public static IApplicationBuilder UseFeatureGate(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.UseMiddleware<FeatureGateMiddleware>();
    }
}
