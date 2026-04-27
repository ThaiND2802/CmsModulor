using Commerce.Modules.Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Commerce.Modules.Catalog.Infrastructure.Persistence.Configurations;

public sealed class ProductVariantOptionConfiguration : IEntityTypeConfiguration<ProductVariantOption>
{
    public void Configure(EntityTypeBuilder<ProductVariantOption> builder)
    {
        builder.HasKey(static x => x.Id);

        builder.HasOne(static x => x.Variant)
            .WithMany(static x => x.Options)
            .HasForeignKey(static x => x.VariantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(static x => x.Attribute)
            .WithMany(static x => x.ProductVariantOptions)
            .HasForeignKey(static x => x.AttributeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
