using Commerce.Modules.Catalog.Domain;
using CommerceCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Commerce.Modules.Catalog.Infrastructure.Persistence.Configurations;

public sealed class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.HasKey(static x => x.Id);

        builder.HasIndex(static x => x.Slug)
            .IsUnique();

        builder.ConfigureAuditAndSoftDelete();
    }
}
