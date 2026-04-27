using Commerce.Modules.Catalog.Domain;
using CommerceCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Commerce.Modules.Catalog.Infrastructure.Persistence.Configurations;

public sealed class ProductAttributeConfiguration : IEntityTypeConfiguration<ProductAttribute>
{
    public void Configure(EntityTypeBuilder<ProductAttribute> builder)
    {
        builder.HasKey(static x => x.Id);

        builder.HasIndex(static x => x.Name)
            .IsUnique();

        builder.ConfigureAuditAndSoftDelete();
    }
}
