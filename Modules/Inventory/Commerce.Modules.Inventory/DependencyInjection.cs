using Commerce.Modules.Inventory.Application.Services;
using Commerce.Modules.Inventory.Contracts;
using Commerce.Modules.Inventory.Infrastructure;
using CommerceCore.Application.Behaviors;
using CommerceCore.FeatureManagement.Abstractions;
using CommerceCore.FeatureManagement.DependencyInjection;
using CommerceCore.FeatureManagement.Models;
using CommerceCore.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Commerce.Modules.Inventory;

public static class DependencyInjection
{
    public static IServiceCollection AddInventoryModule(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        if (!configuration.GetValue<bool>($"{ModuleOptions.SectionName}:Enabled:Catalog"))
        {
            throw new InvalidOperationException("Inventory module requires Catalog module to be enabled.");
        }

        services.AddDbContext<InventoryDbContext>(options =>
            options.UseDefaultDatabase(connectionString));
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        services.AddAutoMapper(cfg => { }, typeof(DependencyInjection).Assembly);
        services.AddScoped<IInventoryModule, InventoryModule>();

        return services;
    }

    public static async Task MigrateInventoryModuleAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        using var scope = serviceProvider.CreateScope();
        var moduleGate = scope.ServiceProvider.GetRequiredService<IModuleGate>();
        if (!moduleGate.IsEnabled("Inventory"))
        {
            return;
        }

        var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
    }

    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        return endpoints.MapCommerceModule("Inventory", "/api/inventory", "Inventory", group =>
        {
            group.MapGet("/health", () => Results.Ok(new
            {
                module = "Inventory",
                status = "Healthy"
            }));
        });
    }
}
