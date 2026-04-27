using Commerce.Modules.Order.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Commerce.Modules.Order.Infrastructure.Persistence.Configurations;

public sealed class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.HasKey(static x => x.Id);

        builder.Property(static x => x.FromStatus)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(static x => x.ToStatus)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(static x => x.ChangedAtUtc)
            .IsRequired();
    }
}
