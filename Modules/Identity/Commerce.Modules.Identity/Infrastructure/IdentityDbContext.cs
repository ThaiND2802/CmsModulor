using CommerceCore.Application.Abstractions;
using CommerceCore.Infrastructure.Persistence;
using Commerce.Modules.Identity.Domain;
using Commerce.Modules.Identity.Infrastructure.Persistence.Seeds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Commerce.Modules.Identity.Infrastructure;

public sealed class IdentityDbContext : BaseDbContext
{
    public IdentityDbContext(
        DbContextOptions<IdentityDbContext> options,
        IDateTimeProvider dateTimeProvider,
        ICurrentUser currentUser)
        : base(options, dateTimeProvider, currentUser)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<Permission> Permissions => Set<Permission>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyIdentityAuthorizationSeed();
    }

    public async Task EnsureSeedDataAsync(CancellationToken cancellationToken = default)
    {
        var databaseCreator = Database.GetService<IRelationalDatabaseCreator>();

        if (!await databaseCreator.ExistsAsync(cancellationToken))
        {
            await Database.EnsureCreatedAsync(cancellationToken);
            return;
        }

        if (await IdentityUsersTableExistsAsync(cancellationToken))
        {
            return;
        }

        await databaseCreator.CreateTablesAsync(cancellationToken);
    }

    private async Task<bool> IdentityUsersTableExistsAsync(CancellationToken cancellationToken)
    {
        await Database.OpenConnectionAsync(cancellationToken);

        try
        {
            await using var command = Database.GetDbConnection().CreateCommand();
            command.CommandText = """
                select exists (
                    select 1
                    from information_schema.tables
                    where table_schema = 'public'
                      and table_name = 'identity_users'
                )
                """;

            var result = await command.ExecuteScalarAsync(cancellationToken);
            return result is true;
        }
        finally
        {
            await Database.CloseConnectionAsync();
        }
    }
}
