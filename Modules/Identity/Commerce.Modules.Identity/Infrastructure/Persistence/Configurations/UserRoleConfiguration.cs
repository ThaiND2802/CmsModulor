using CommerceCore.Infrastructure.Persistence;
using Commerce.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Commerce.Modules.Identity.Infrastructure.Persistence.Configurations;

public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");
        builder.HasKey(static x => x.Id);

        builder.Property(static x => x.AssignedAtUtc)
            .IsRequired();

        builder.Property(static x => x.AssignedBy)
            .HasMaxLength(100);

        builder.Property(static x => x.ExpiresAtUtc);

        builder.Property(static x => x.IsActive)
            .IsRequired();

        builder.HasIndex(static x => new { x.UserId, x.RoleId })
            .IsUnique();

        builder.HasOne(static x => x.User)
            .WithMany(static x => x.UserRoles)
            .HasForeignKey(static x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(static x => x.Role)
            .WithMany(static x => x.UserRoles)
            .HasForeignKey(static x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAuditAndSoftDelete();
    }
}
