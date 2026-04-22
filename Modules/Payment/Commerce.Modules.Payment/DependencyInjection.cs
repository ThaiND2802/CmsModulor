using Commerce.Modules.Payment.Application.Commands.CreateCodCheckout;
using Commerce.Modules.Payment.Application.Commands.CreatePaymentMethod;
using Commerce.Modules.Payment.Application.Commands.DeletePaymentMethod;
using Commerce.Modules.Payment.Application.Commands.UpdatePaymentMethod;
using Commerce.Modules.Payment.Application.Queries.GetCodHealth;
using Commerce.Modules.Payment.Application.Queries.GetDeletedPaymentMethods;
using Commerce.Modules.Payment.Application.Queries.GetMyCodPermissions;
using Commerce.Modules.Payment.Application.Queries.GetPaymentMethodById;
using Commerce.Modules.Payment.Application.Queries.GetPaymentMethods;
using Commerce.Modules.Payment.Controllers;
using Commerce.Modules.Payment.Infrastructure;
using CommerceCore.FeatureManagement.Abstractions;
using CommerceCore.Infrastructure.Persistence;
using CommerceCore.Infrastructure.Persistence.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Commerce.Modules.Payment;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentModule(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<PaymentDbContext>(options =>
        {
            options.UseDefaultDatabase(connectionString);
        });
        services.AddScoped<IUnitOfWork, UnitOfWork<PaymentDbContext>>();
        services.AddScoped<GetPaymentMethodsHandler>();
        services.AddScoped<GetPaymentMethodByIdHandler>();
        services.AddScoped<GetDeletedPaymentMethodsHandler>();
        services.AddScoped<GetCodHealthHandler>();
        services.AddScoped<GetMyCodPermissionsHandler>();
        services.AddScoped<CreatePaymentMethodHandler>();
        services.AddScoped<UpdatePaymentMethodHandler>();
        services.AddScoped<DeletePaymentMethodHandler>();
        services.AddScoped<CreateCodCheckoutHandler>();

        return services;
    }

    public static IServiceProvider InitializePaymentPersistence(this IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        using var scope = serviceProvider.CreateScope();
        var moduleGate = scope.ServiceProvider.GetRequiredService<IModuleGate>();
        if (!moduleGate.IsEnabled("Payment"))
        {
            return serviceProvider;
        }

        var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
        dbContext.EnsureSeedDataAsync().GetAwaiter().GetResult();

        return serviceProvider;
    }
}
