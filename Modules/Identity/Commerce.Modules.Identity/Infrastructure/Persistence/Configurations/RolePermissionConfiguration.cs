using CommerceCore.Infrastructure.Persistence;
using Commerce.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Commerce.Modules.Identity.Infrastructure.Persistence.Configurations;

public sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions");
        builder.HasKey(static x => x.Id);

        builder.Property(static x => x.Effect)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(static x => x.AssignedAtUtc)
            .IsRequired();

        builder.Property(static x => x.AssignedBy)
            .HasMaxLength(100);

        builder.HasIndex(static x => new { x.RoleId, x.PermissionId })
            .IsUnique();

        builder.HasOne(static x => x.Role)
            .WithMany(static x => x.RolePermissions)
            .HasForeignKey(static x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(static x => x.Permission)
            .WithMany(static x => x.RolePermissions)
            .HasForeignKey(static x => x.PermissionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAuditAndSoftDelete();
    }
}
