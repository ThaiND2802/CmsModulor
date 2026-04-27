using Commerce.Modules.Sale.Application.Services;
using Commerce.Modules.Sale.Infrastructure;
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

namespace Commerce.Modules.Sale;

public static class DependencyInjection
{
    public static IServiceCollection AddSaleModule(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<SaleDbContext>(options =>
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
        services.AddScoped<ISalePricingService, DefaultSalePricingService>();
        services.AddScoped<ISaleCouponService, InMemorySaleCouponService>();
        services.AddScoped<SaleSubmissionService>();
        services.AddSingleton<Application.Policies.SaleChannelPolicyFactory>();

        return services;
    }

    public static async Task MigrateSaleModuleAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        using var scope = serviceProvider.CreateScope();
        var moduleGate = scope.ServiceProvider.GetRequiredService<IModuleGate>();
        if (!moduleGate.IsEnabled("Sale"))
        {
            return;
        }

        var dbContext = scope.ServiceProvider.GetRequiredService<SaleDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
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
