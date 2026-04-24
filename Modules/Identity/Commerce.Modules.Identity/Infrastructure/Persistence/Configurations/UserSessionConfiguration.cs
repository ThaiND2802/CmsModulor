using CommerceCore.Infrastructure.Persistence;
using Commerce.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Commerce.Modules.Identity.Infrastructure.Persistence.Configurations;

public sealed class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.HasKey(static x => x.Id);

        builder.HasIndex(static x => x.RefreshTokenHash)
            .IsUnique();

        builder.HasIndex(static x => x.UserId);

        builder.HasOne(static x => x.User)
            .WithMany(static x => x.Sessions)
            .HasForeignKey(static x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAuditAndSoftDelete();
    }
}
