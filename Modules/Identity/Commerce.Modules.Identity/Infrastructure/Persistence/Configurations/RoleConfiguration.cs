using CommerceCore.Infrastructure.Persistence;
using Commerce.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Commerce.Modules.Identity.Infrastructure.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(static x => x.Id);

        builder.Property(static x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(static x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(static x => x.Description)
            .HasMaxLength(1000);

        builder.Property(static x => x.IsActive)
            .IsRequired();

        builder.Property(static x => x.IsSystem)
            .IsRequired();

        builder.HasIndex(static x => x.Code)
            .IsUnique();

        builder.ConfigureAuditAndSoftDelete();
    }
}
