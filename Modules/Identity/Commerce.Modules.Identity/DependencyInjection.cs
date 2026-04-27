using Commerce.Modules.Identity.Infrastructure;
using Commerce.Modules.Identity.Infrastructure.Authentication;
using Commerce.Modules.Identity.Infrastructure.Security;
using CommerceCore.Application.Behaviors;
using CommerceCore.FeatureManagement.Abstractions;
using CommerceCore.Infrastructure.Persistence;
using CommerceCore.Infrastructure.Persistence.Abstractions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Commerce.Modules.Identity;

public static class DependencyInjection
{
    private const string InitialIdentityMigrationId = "20260424070027_InitialIdentity";
    private const string EfProductVersion = "8.0.11";
    private const string RepairIdentityMigrationHistorySetting = "Startup:RepairIdentityMigrationHistory";

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
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        return services;
    }

    public static async Task MigrateIdentityModuleAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        using var scope = serviceProvider.CreateScope();
        var moduleGate = scope.ServiceProvider.GetRequiredService<IModuleGate>();
        if (!moduleGate.IsEnabled("Identity"))
        {
            return;
        }

        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        if (configuration.GetValue<bool>(RepairIdentityMigrationHistorySetting))
        {
            await EnsureIdentityMigrationHistoryAsync(dbContext, cancellationToken);
        }

        await dbContext.Database.MigrateAsync(cancellationToken);
    }

    private static async Task EnsureIdentityMigrationHistoryAsync(IdentityDbContext dbContext, CancellationToken cancellationToken)
    {
        var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync(cancellationToken);
        if (appliedMigrations.Contains(InitialIdentityMigrationId, StringComparer.Ordinal))
        {
            return;
        }

        if (!await IdentitySchemaExistsAsync(dbContext, cancellationToken))
        {
            return;
        }

        var historyRepository = dbContext.GetService<IHistoryRepository>();
        var createHistoryTableScript = historyRepository.GetCreateIfNotExistsScript();
        if (!string.IsNullOrWhiteSpace(createHistoryTableScript))
        {
            await dbContext.Database.ExecuteSqlRawAsync(createHistoryTableScript, cancellationToken);
        }

        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            SELECT {InitialIdentityMigrationId}, {EfProductVersion}
            WHERE NOT EXISTS (
                SELECT 1
                FROM "__EFMigrationsHistory"
                WHERE "MigrationId" = {InitialIdentityMigrationId}
            );
            """,
            cancellationToken);
    }

    private static async Task<bool> IdentitySchemaExistsAsync(IdentityDbContext dbContext, CancellationToken cancellationToken)
    {
        var database = dbContext.Database;
        var connectionWasClosed = database.GetDbConnection().State != System.Data.ConnectionState.Open;

        if (connectionWasClosed)
        {
            await database.OpenConnectionAsync(cancellationToken);
        }

        try
        {
            await using var command = database.GetDbConnection().CreateCommand();
            command.CommandText =
                """
                SELECT EXISTS (
                    SELECT 1
                    FROM information_schema.tables
                    WHERE table_schema = 'public'
                      AND table_name IN ('identity_permissions', 'identity_roles', 'identity_users')
                );
                """;

            var result = await command.ExecuteScalarAsync(cancellationToken);
            return result is bool value && value;
        }
        finally
        {
            if (connectionWasClosed)
            {
                await database.CloseConnectionAsync();
            }
        }
    }
}
