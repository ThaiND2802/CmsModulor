using CommerceCore.Infrastructure.Persistence;
using Commerce.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Commerce.Modules.Identity.Infrastructure.Persistence.Configurations;

public sealed class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermission>
{
    public void Configure(EntityTypeBuilder<UserPermission> builder)
    {
        builder.ToTable("UserPermissions");
        builder.HasKey(static x => x.Id);

        builder.Property(static x => x.Effect)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(static x => x.AssignedAtUtc)
            .IsRequired();

        builder.Property(static x => x.AssignedBy)
            .HasMaxLength(100);

        builder.Property(static x => x.ExpiresAtUtc);

        builder.HasIndex(static x => new { x.UserId, x.PermissionId })
            .IsUnique();

        builder.HasOne(static x => x.User)
            .WithMany(static x => x.UserPermissions)
            .HasForeignKey(static x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(static x => x.Permission)
            .WithMany(static x => x.UserPermissions)
            .HasForeignKey(static x => x.PermissionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAuditAndSoftDelete();
    }
}
