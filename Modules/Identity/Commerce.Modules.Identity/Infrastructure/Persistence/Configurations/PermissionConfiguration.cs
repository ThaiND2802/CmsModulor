using CommerceCore.Infrastructure.Persistence;
using Commerce.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Commerce.Modules.Identity.Infrastructure.Persistence.Configurations;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");
        builder.HasKey(static x => x.Id);

        builder.Property(static x => x.Code)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(static x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(static x => x.Description)
            .HasMaxLength(1000);

        builder.Property(static x => x.Module)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(static x => x.Feature)
            .HasMaxLength(100);

        builder.Property(static x => x.GroupName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(static x => x.SortOrder)
            .IsRequired();

        builder.Property(static x => x.IsActive)
            .IsRequired();

        builder.Property(static x => x.IsSystem)
            .IsRequired();

        builder.HasIndex(static x => x.Code)
            .IsUnique();

        builder.HasIndex(static x => new { x.Module, x.Feature });

        builder.HasIndex(static x => x.GroupName);

        builder.ConfigureAuditAndSoftDelete();
    }
}
