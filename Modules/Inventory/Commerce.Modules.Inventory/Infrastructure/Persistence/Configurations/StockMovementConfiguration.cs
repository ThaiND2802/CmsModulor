using Commerce.Modules.Inventory.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Commerce.Modules.Inventory.Infrastructure.Persistence.Configurations;

public sealed class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.HasKey(static x => x.Id);

        builder.Property(static x => x.MovementType)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(static x => x.Reason)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(static x => x.ReferenceType)
            .HasMaxLength(100);

        builder.HasIndex(static x => x.InventoryItemId);
        builder.HasIndex(static x => new { x.ReferenceType, x.ReferenceId });

        builder.HasOne(static x => x.InventoryItem)
            .WithMany(static x => x.StockMovements)
            .HasForeignKey(static x => x.InventoryItemId);

        builder.ToTable(table =>
        {
            table.HasCheckConstraint("ck_inventory_stock_movements_on_hand_after_non_negative", "on_hand_after >= 0");
            table.HasCheckConstraint("ck_inventory_stock_movements_reserved_after_non_negative", "reserved_after >= 0");
            table.HasCheckConstraint("ck_inventory_stock_movements_reserved_after_within_on_hand", "reserved_after <= on_hand_after");
        });
    }
}
