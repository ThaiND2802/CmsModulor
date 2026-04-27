using Commerce.Modules.Sale.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Commerce.Modules.Sale.Infrastructure.Persistence.Configurations;

public sealed class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.HasKey(static x => x.Id);

        builder.Property(static x => x.ProductName)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(static x => x.ProductSku)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(static x => x.VariantName)
            .HasMaxLength(300);

        builder.Property(static x => x.UnitPrice).HasPrecision(18, 4);
        builder.Property(static x => x.OverridePrice).HasPrecision(18, 4);
        builder.Property(static x => x.OverrideReason).HasMaxLength(500);
        builder.Property(static x => x.OverriddenBy).HasMaxLength(100);
        builder.Property(static x => x.DiscountAmount).HasPrecision(18, 4);
        builder.Property(static x => x.TotalAmount).HasPrecision(18, 4);
    }
}
