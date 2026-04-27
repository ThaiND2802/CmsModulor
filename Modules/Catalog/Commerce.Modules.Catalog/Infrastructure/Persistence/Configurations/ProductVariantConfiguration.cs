using Commerce.Modules.Catalog.Domain;
using CommerceCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Commerce.Modules.Catalog.Infrastructure.Persistence.Configurations;

public sealed class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.HasKey(static x => x.Id);

        builder.HasIndex(static x => x.Sku)
            .IsUnique();

        builder.HasOne(static x => x.Product)
            .WithMany(static x => x.Variants)
            .HasForeignKey(static x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ConfigureAuditAndSoftDelete();
    }
}
