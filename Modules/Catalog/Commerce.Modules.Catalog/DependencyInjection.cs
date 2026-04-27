using Commerce.Modules.Catalog.Application.Services;
using Commerce.Modules.Catalog.Contracts;
using Commerce.Modules.Catalog.Infrastructure;
using CommerceCore.Application.Behaviors;
using CommerceCore.FeatureManagement.Abstractions;
using CommerceCore.FeatureManagement.DependencyInjection;
using CommerceCore.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Commerce.Modules.Catalog;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogModule(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<CatalogDbContext>(options =>
        {
            options.UseDefaultDatabase(connectionString);
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        services.AddAutoMapper(cfg => { }, typeof(DependencyInjection).Assembly);
        services.AddScoped<ICatalogVariantLookup, CatalogVariantLookup>();

        return services;
    }

    public static async Task MigrateCatalogModuleAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        using var scope = serviceProvider.CreateScope();
        var moduleGate = scope.ServiceProvider.GetRequiredService<IModuleGate>();
        if (!moduleGate.IsEnabled("Catalog"))
        {
            return;
        }

        var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
    }

    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        return endpoints.MapCommerceModule("Catalog", "/api/catalog", "Catalog", group =>
        {
            group.MapGet("/health", () => Results.Ok(new
            {
                module = "Catalog",
                status = "Healthy"
            }));
        });
    }
}
