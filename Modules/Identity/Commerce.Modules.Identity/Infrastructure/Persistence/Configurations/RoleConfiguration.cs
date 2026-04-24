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

        builder.HasIndex(static x => x.Code)
            .IsUnique();

        builder.ConfigureAuditAndSoftDelete();
    }
}
