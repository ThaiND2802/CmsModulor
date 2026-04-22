using CommerceCore.Infrastructure.Persistence;
using Commerce.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Commerce.Modules.Identity.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(static x => x.Id);

        builder.Property(static x => x.UserName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(static x => x.NormalizedUserName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(static x => x.DisplayName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(static x => x.Email)
            .HasMaxLength(256);

        builder.Property(static x => x.NormalizedEmail)
            .HasMaxLength(256);

        builder.Property(static x => x.PasswordHash)
            .HasMaxLength(512);

        builder.Property(static x => x.PhoneNumber)
            .HasMaxLength(50);

        builder.Property(static x => x.IsActive)
            .IsRequired();

        builder.Property(static x => x.IsSystem)
            .IsRequired();

        builder.Property(static x => x.LastLoginAtUtc);

        builder.HasIndex(static x => x.NormalizedUserName)
            .IsUnique();

        builder.HasIndex(static x => x.NormalizedEmail);

        builder.ConfigureAuditAndSoftDelete();
    }
}
