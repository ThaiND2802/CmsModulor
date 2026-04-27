using Commerce.Modules.Inventory.Domain;
using CommerceCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Commerce.Modules.Inventory.Infrastructure.Persistence.Configurations;

public sealed class StockReservationConfiguration : IEntityTypeConfiguration<StockReservation>
{
    public void Configure(EntityTypeBuilder<StockReservation> builder)
    {
        builder.HasKey(static x => x.Id);

        builder.HasIndex(static x => x.OrderId)
            .IsUnique();

        builder.Property(static x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.HasMany(static x => x.Items)
            .WithOne(static x => x.StockReservation)
            .HasForeignKey(static x => x.StockReservationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ConfigureAuditAndSoftDelete();
    }
}
