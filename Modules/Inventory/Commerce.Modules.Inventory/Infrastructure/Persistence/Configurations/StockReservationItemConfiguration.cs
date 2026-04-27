using Commerce.Modules.Inventory.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Commerce.Modules.Inventory.Infrastructure.Persistence.Configurations;

public sealed class StockReservationItemConfiguration : IEntityTypeConfiguration<StockReservationItem>
{
    public void Configure(EntityTypeBuilder<StockReservationItem> builder)
    {
        builder.HasKey(static x => x.Id);

        builder.HasIndex(static x => new { x.StockReservationId, x.InventoryItemId })
            .IsUnique();

        builder.HasOne(static x => x.StockReservation)
            .WithMany(static x => x.Items)
            .HasForeignKey(static x => x.StockReservationId);

        builder.HasOne(static x => x.InventoryItem)
            .WithMany(static x => x.ReservationItems)
            .HasForeignKey(static x => x.InventoryItemId);

        builder.ToTable(table =>
        {
            table.HasCheckConstraint("ck_inventory_stock_reservation_items_quantity_positive", "quantity > 0");
        });
    }
}
