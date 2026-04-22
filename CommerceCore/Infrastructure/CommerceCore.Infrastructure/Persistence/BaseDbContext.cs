using System.Linq.Expressions;
using CommerceCore.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CommerceCore.Infrastructure.Persistence;

public abstract class BaseDbContext : DbContext
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICurrentUser _currentUser;

    protected BaseDbContext(
        DbContextOptions options,
        IDateTimeProvider dateTimeProvider,
        ICurrentUser currentUser)
        : base(options)
    {
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyPersistenceConventions();

        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyPersistenceConventions();

        return base.SaveChanges();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        modelBuilder.ApplySoftDeleteQueryFilters();
        modelBuilder.ApplyRestrictiveDeleteBehavior();
    }

    private void ApplyPersistenceConventions()
    {
        HandleSoftDelete();
        HandleAudit();
    }

    private void HandleAudit()
    {
        var utcNow = _dateTimeProvider.UtcNow;
        var userId = _currentUser.UserId;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added && entry.Entity is ICreatedAuditable createdAuditable)
            {
                createdAuditable.CreatedAtUtc = utcNow;
                createdAuditable.CreatedBy = userId;
            }

            if (entry.State == EntityState.Modified && entry.Entity is IUpdatedAuditable updatedAuditable)
            {
                if (entry.Entity is ICreatedAuditable)
                {
                    entry.Property(nameof(ICreatedAuditable.CreatedAtUtc)).IsModified = false;
                    entry.Property(nameof(ICreatedAuditable.CreatedBy)).IsModified = false;
                }

                updatedAuditable.UpdatedAtUtc = utcNow;
                updatedAuditable.UpdatedBy = userId;
            }
        }
    }

    private void HandleSoftDelete()
    {
        var utcNow = _dateTimeProvider.UtcNow;
        var userId = _currentUser.UserId;

        foreach (var entry in ChangeTracker.Entries<ISoftDelete>().Where(static x => x.State == EntityState.Deleted))
        {
            entry.State = EntityState.Modified;
            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedAtUtc = utcNow;
            entry.Entity.DeletedBy = userId;

            if (entry.Entity is IUpdatedAuditable updatedAuditable)
            {
                updatedAuditable.UpdatedAtUtc = utcNow;
                updatedAuditable.UpdatedBy = userId;
            }
        }
    }
}

public static class ModelBuilderSoftDeleteExtensions
{
    public static void ApplySoftDeleteQueryFilters(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            modelBuilder.Entity(entityType.ClrType)
                .HasQueryFilter(BuildSoftDeleteFilter(entityType.ClrType));
        }
    }

    public static void ApplyRestrictiveDeleteBehavior(this ModelBuilder modelBuilder)
    {
        foreach (var foreignKey in modelBuilder.Model
                     .GetEntityTypes()
                     .SelectMany(static entityType => entityType.GetForeignKeys()))
        {
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }

    private static LambdaExpression BuildSoftDeleteFilter(Type entityClrType)
    {
        var parameter = Expression.Parameter(entityClrType, "entity");
        var property = Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
        var compare = Expression.Equal(property, Expression.Constant(false));

        return Expression.Lambda(compare, parameter);
    }
}
