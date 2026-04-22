using CommerceCore.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommerceCore.Infrastructure.Persistence;

public static class EntityTypeBuilderExtensions
{
    public static void ConfigureAuditAndSoftDelete<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (typeof(ICreatedAuditable).IsAssignableFrom(typeof(TEntity)))
        {
            builder.Property<DateTime>(nameof(ICreatedAuditable.CreatedAtUtc))
                .IsRequired();

            builder.Property<string?>(nameof(ICreatedAuditable.CreatedBy))
                .HasMaxLength(100);
        }

        if (typeof(IUpdatedAuditable).IsAssignableFrom(typeof(TEntity)))
        {
            builder.Property<DateTime?>(nameof(IUpdatedAuditable.UpdatedAtUtc));

            builder.Property<string?>(nameof(IUpdatedAuditable.UpdatedBy))
                .HasMaxLength(100);
        }

        if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
        {
            builder.Property<bool>(nameof(ISoftDelete.IsDeleted));

            builder.Property<DateTime?>(nameof(ISoftDelete.DeletedAtUtc));

            builder.Property<string?>(nameof(ISoftDelete.DeletedBy))
                .HasMaxLength(100);
        }
    }
}
