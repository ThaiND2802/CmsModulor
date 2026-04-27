using Commerce.Modules.Inventory.Domain;
using CommerceCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Commerce.Modules.Inventory.Infrastructure.Persistence.Configurations;

public sealed class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.HasKey(static x => x.Id);

        builder.Property(static x => x.Sku)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(static x => x.VariantId)
            .IsUnique();

        builder.HasIndex(static x => x.Sku);

        builder.HasMany(static x => x.StockMovements)
            .WithOne(static x => x.InventoryItem)
            .HasForeignKey(static x => x.InventoryItemId);

        builder.HasMany(static x => x.ReservationItems)
            .WithOne(static x => x.InventoryItem)
            .HasForeignKey(static x => x.InventoryItemId);

        builder.Ignore(static x => x.AvailableQuantity);

        builder.ToTable(table =>
        {
            table.HasCheckConstraint("ck_inventory_items_on_hand_quantity_non_negative", "on_hand_quantity >= 0");
            table.HasCheckConstraint("ck_inventory_items_reserved_quantity_non_negative", "reserved_quantity >= 0");
            table.HasCheckConstraint("ck_inventory_items_reserved_quantity_within_on_hand", "reserved_quantity <= on_hand_quantity");
        });

        builder.ConfigureAuditAndSoftDelete();
    }
}
