using Commerce.Modules.Inventory.Domain;
using CommerceCore.Application.Abstractions;
using CommerceCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Commerce.Modules.Inventory.Infrastructure;

public sealed class InventoryDbContext : BaseDbContext
{
    public InventoryDbContext(
        DbContextOptions<InventoryDbContext> options,
        IDateTimeProvider dateTimeProvider,
        ICurrentUser currentUser)
        : base(options, dateTimeProvider, currentUser)
    {
    }

    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();

    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    public DbSet<StockReservation> StockReservations => Set<StockReservation>();

    public DbSet<StockReservationItem> StockReservationItems => Set<StockReservationItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
