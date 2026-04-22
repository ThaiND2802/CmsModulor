using CommerceCore.FeatureManagement.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace CommerceCore.FeatureManagement.DependencyInjection;

public static class PermissionEndpointExtensions
{
    public static TBuilder RequirePermission<TBuilder>(this TBuilder builder, string permission)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(permission);

        builder.RequireAuthorization(new AuthorizeAttribute
        {
            Policy = $"{PermissionAuthorizeAttribute.PolicyPrefix}:{permission}"
        });

        return builder;
    }

    public static TBuilder RequirePermissions<TBuilder>(this TBuilder builder, params string[] permissions)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(permissions);

        foreach (var permission in permissions.Where(static x => !string.IsNullOrWhiteSpace(x)))
        {
            builder.RequirePermission(permission);
        }

        return builder;
    }

    public static RouteGroupBuilder MapCommercePermissionGroup(
        this IEndpointRouteBuilder endpoints,
        string routePrefix,
        string permission)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentException.ThrowIfNullOrWhiteSpace(routePrefix);
        ArgumentException.ThrowIfNullOrWhiteSpace(permission);

        return endpoints.MapGroup(routePrefix)
            .RequirePermission(permission);
    }

    public static RouteGroupBuilder MapCommercePermissionGroup(
        this RouteGroupBuilder group,
        string routePrefix,
        string permission)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentException.ThrowIfNullOrWhiteSpace(routePrefix);
        ArgumentException.ThrowIfNullOrWhiteSpace(permission);

        return group.MapGroup(routePrefix)
            .RequirePermission(permission);
    }
}
