using Commerce.Modules.Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Commerce.Modules.Catalog.Infrastructure.Persistence.Configurations;

public sealed class ProductMediaConfiguration : IEntityTypeConfiguration<ProductMedia>
{
    public void Configure(EntityTypeBuilder<ProductMedia> builder)
    {
        builder.HasKey(static x => x.Id);

        builder.HasOne(static x => x.Product)
            .WithMany(static x => x.Media)
            .HasForeignKey(static x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
