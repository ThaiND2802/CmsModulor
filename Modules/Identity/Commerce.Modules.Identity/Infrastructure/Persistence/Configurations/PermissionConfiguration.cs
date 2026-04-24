using CommerceCore.Infrastructure.Persistence;
using Commerce.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Commerce.Modules.Identity.Infrastructure.Persistence.Configurations;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.HasKey(static x => x.Id);

        builder.HasIndex(static x => x.Code)
            .IsUnique();

        builder.HasIndex(static x => new { x.Module, x.Feature });

        builder.HasIndex(static x => x.GroupName);

        builder.ConfigureAuditAndSoftDelete();
    }
}
