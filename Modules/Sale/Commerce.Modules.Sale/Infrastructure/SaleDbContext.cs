using Commerce.Modules.Sale.Domain;
using CommerceCore.Application.Abstractions;
using CommerceCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SaleEntity = Commerce.Modules.Sale.Domain.Sale;

namespace Commerce.Modules.Sale.Infrastructure;

public sealed class SaleDbContext : BaseDbContext
{
    public SaleDbContext(
        DbContextOptions<SaleDbContext> options,
        IDateTimeProvider dateTimeProvider,
        ICurrentUser currentUser)
        : base(options, dateTimeProvider, currentUser)
    {
    }

    public DbSet<SaleEntity> Sales => Set<SaleEntity>();

    public DbSet<SaleItem> SaleItems => Set<SaleItem>();

    public DbSet<SaleStatusHistory> SaleStatusHistory => Set<SaleStatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SaleItem>()
            .HasQueryFilter(static item => !item.Sale.IsDeleted);
        modelBuilder.Entity<SaleStatusHistory>()
            .HasQueryFilter(static history => !history.Sale.IsDeleted);
    }
}
