using CommerceCore.Infrastructure.Persistence;
using Commerce.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Commerce.Modules.Identity.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(static x => x.Id);

        builder.HasIndex(static x => x.NormalizedUserName)
            .IsUnique();

        builder.HasIndex(static x => x.NormalizedEmail);

        builder.ConfigureAuditAndSoftDelete();
    }
}
