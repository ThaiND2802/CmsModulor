using Commerce.Modules.Catalog.Domain;
using CommerceCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Commerce.Modules.Catalog.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(static x => x.Id);

        builder.HasIndex(static x => x.Slug)
            .IsUnique();

        builder.HasIndex(static x => x.Sku)
            .IsUnique();

        builder.HasOne(static x => x.Category)
            .WithMany(static x => x.Products)
            .HasForeignKey(static x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(static x => x.Brand)
            .WithMany(static x => x.Products)
            .HasForeignKey(static x => x.BrandId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.ConfigureAuditAndSoftDelete();
    }
}
