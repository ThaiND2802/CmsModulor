using CommerceCore.Application.Abstractions;
using CommerceCore.Infrastructure.Persistence;
using Commerce.Modules.Identity.Infrastructure.Authentication;
using Commerce.Modules.Identity.Domain;
using Commerce.Modules.Identity.Infrastructure.Persistence.Seeds;
using Microsoft.EntityFrameworkCore;

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

    public Task EnsureSeedDataAsync(CancellationToken cancellationToken = default)
    {
        return EnsureSeedDataInternalAsync(cancellationToken);
    }

    private async Task EnsureSeedDataInternalAsync(CancellationToken cancellationToken)
    {
        await Database.MigrateAsync(cancellationToken);
        await SynchronizeSeedPasswordsAsync(cancellationToken);
    }

    private async Task SynchronizeSeedPasswordsAsync(CancellationToken cancellationToken)
    {
        var passwordHasher = new PasswordHasherService();
        var seededUserIds = SystemSeed.SeedUserPasswords
            .Select(static x => x.UserId)
            .ToArray();

        var users = await Users
            .Where(x => seededUserIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var updated = false;

        foreach (var (userId, password) in SystemSeed.SeedUserPasswords)
        {
            if (!users.TryGetValue(userId, out var user))
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(user.PasswordHash) && passwordHasher.Verify(user.PasswordHash, password))
            {
                continue;
            }

            user.PasswordHash = passwordHasher.Hash(password);
            updated = true;
        }

        if (updated)
        {
            await SaveChangesAsync(cancellationToken);
        }
    }
}
