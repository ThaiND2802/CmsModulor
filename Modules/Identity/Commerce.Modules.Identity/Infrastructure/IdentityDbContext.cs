using CommerceCore.Application.Abstractions;
using CommerceCore.Infrastructure.Persistence;
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

    public DbSet<UserSession> UserSessions => Set<UserSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<UserSession>()
            .HasQueryFilter(static session => !session.User.IsDeleted);
        modelBuilder.ApplyIdentityAuthorizationSeed();
    }
}
