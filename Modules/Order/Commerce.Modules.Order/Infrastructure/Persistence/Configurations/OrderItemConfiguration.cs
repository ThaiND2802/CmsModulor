using Commerce.Modules.Order.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Commerce.Modules.Order.Infrastructure.Persistence.Configurations;

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(static x => x.Id);

        builder.Property(static x => x.UnitPrice)
            .HasPrecision(18, 4);

        builder.Property(static x => x.DiscountAmount)
            .HasPrecision(18, 4);

        builder.Property(static x => x.TotalAmount)
            .HasPrecision(18, 4);
    }
}
