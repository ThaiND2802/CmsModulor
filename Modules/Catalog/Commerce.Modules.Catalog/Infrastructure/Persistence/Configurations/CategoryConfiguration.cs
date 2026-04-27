using Commerce.Modules.Catalog.Domain;
using CommerceCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Commerce.Modules.Catalog.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(static x => x.Id);

        builder.HasIndex(static x => x.Slug)
            .IsUnique();

        builder.HasOne(static x => x.Parent)
            .WithMany(static x => x.Children)
            .HasForeignKey(static x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAuditAndSoftDelete();
    }
}
