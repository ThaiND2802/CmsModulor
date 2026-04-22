using Commerce.Modules.Identity.Application.Commands.Login;
using Commerce.Modules.Identity.Application.Queries.GetAuthorizationOverview;
using Commerce.Modules.Identity.Application.Queries.GetUsers;
using Commerce.Modules.Identity.Controllers;
using Commerce.Modules.Identity.Infrastructure;
using Commerce.Modules.Identity.Infrastructure.Authentication;
using Commerce.Modules.Identity.Infrastructure.Security;
using CommerceCore.FeatureManagement.Abstractions;
using CommerceCore.Infrastructure.Persistence;
using CommerceCore.Infrastructure.Persistence.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Commerce.Modules.Identity;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<IdentityDbContext>(options =>
        {
            options.UseDefaultDatabase(connectionString);
        });
        services.AddScoped<IUnitOfWork, UnitOfWork<IdentityDbContext>>();
        services.AddScoped<IPermissionGate, DbPermissionGate>();
        services.AddScoped<IPasswordHasherService, PasswordHasherService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        });

        return services;
    }

    public static IServiceProvider InitializeIdentityPersistence(this IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        using var scope = serviceProvider.CreateScope();
        var moduleGate = scope.ServiceProvider.GetRequiredService<IModuleGate>();
        if (!moduleGate.IsEnabled("Identity"))
        {
            return serviceProvider;
        }

        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        dbContext.EnsureSeedDataAsync().GetAwaiter().GetResult();

        return serviceProvider;
    }
}
