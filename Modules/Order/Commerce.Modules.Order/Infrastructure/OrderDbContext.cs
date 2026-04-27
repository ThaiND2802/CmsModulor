using CommerceCore.Application.Abstractions;
using CommerceCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using OrderEntity = Commerce.Modules.Order.Domain.Order;
using OrderItemEntity = Commerce.Modules.Order.Domain.OrderItem;
using OrderStatusHistoryEntity = Commerce.Modules.Order.Domain.OrderStatusHistory;

namespace Commerce.Modules.Order.Infrastructure;

public sealed class OrderDbContext : BaseDbContext
{
    public OrderDbContext(
        DbContextOptions<OrderDbContext> options,
        IDateTimeProvider dateTimeProvider,
        ICurrentUser currentUser)
        : base(options, dateTimeProvider, currentUser)
    {
    }

    public DbSet<OrderEntity> Orders => Set<OrderEntity>();

    public DbSet<OrderItemEntity> OrderItems => Set<OrderItemEntity>();

    public DbSet<OrderStatusHistoryEntity> OrderStatusHistory => Set<OrderStatusHistoryEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<OrderItemEntity>()
            .HasQueryFilter(static item => !item.Order.IsDeleted);
        modelBuilder.Entity<OrderStatusHistoryEntity>()
            .HasQueryFilter(static history => !history.Order.IsDeleted);
    }
}
