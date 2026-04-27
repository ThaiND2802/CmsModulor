using Commerce.Modules.Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Commerce.Modules.Catalog.Infrastructure.Persistence.Configurations;

public sealed class ProductAttributeValueConfiguration : IEntityTypeConfiguration<ProductAttributeValue>
{
    public void Configure(EntityTypeBuilder<ProductAttributeValue> builder)
    {
        builder.HasKey(static x => x.Id);

        builder.HasIndex(static x => new { x.ProductId, x.AttributeId })
            .IsUnique();

        builder.HasOne(static x => x.Product)
            .WithMany(static x => x.AttributeValues)
            .HasForeignKey(static x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(static x => x.Attribute)
            .WithMany(static x => x.ProductAttributeValues)
            .HasForeignKey(static x => x.AttributeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
