using CommerceCore.Application.Behaviors;
using CommerceCore.FeatureManagement.Abstractions;
using CommerceCore.FeatureManagement.DependencyInjection;
using CommerceCore.FeatureManagement.Models;
using CommerceCore.Infrastructure.Persistence;
using CommerceCore.Infrastructure.Persistence.Abstractions;
using Commerce.Modules.Order.Application.Services;
using Commerce.Modules.Order.Contracts;
using Commerce.Modules.Order.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Commerce.Modules.Order;

public static class DependencyInjection
{
    public static IServiceCollection AddOrderModule(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        if (!configuration.GetValue<bool>($"{ModuleOptions.SectionName}:Enabled:Inventory"))
        {
            throw new InvalidOperationException("Order module requires Inventory module to be enabled.");
        }

        services.AddDbContext<OrderDbContext>(options =>
            options.UseDefaultDatabase(connectionString));
        services.AddScoped<IUnitOfWork, UnitOfWork<OrderDbContext>>();
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        services.AddAutoMapper(cfg => { }, typeof(DependencyInjection).Assembly);
        services.AddScoped<IOrderModule, OrderModule>();
        return services;
    }

    public static async Task MigrateOrderModuleAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        using var scope = serviceProvider.CreateScope();
        var moduleGate = scope.ServiceProvider.GetRequiredService<IModuleGate>();
        if (!moduleGate.IsEnabled("Order"))
        {
            return;
        }

        var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
    }

    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        return endpoints.MapCommerceModule("Order", "/api/order", "Order", group =>
        {
            group.MapGet("/health", () => Results.Ok(new
            {
                module = "Order",
                status = "Healthy"
            }));
        });
    }
}
