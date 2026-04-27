using CommerceCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderEntity = Commerce.Modules.Order.Domain.Order;

namespace Commerce.Modules.Order.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<OrderEntity>
{
    public void Configure(EntityTypeBuilder<OrderEntity> builder)
    {
        builder.HasKey(static x => x.Id);

        builder.Property(static x => x.OrderNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(static x => x.OrderNumber)
            .IsUnique();

        builder.Property(static x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(static x => x.SubtotalAmount)
            .HasPrecision(18, 4);

        builder.Property(static x => x.DiscountAmount)
            .HasPrecision(18, 4);

        builder.Property(static x => x.ShippingAmount)
            .HasPrecision(18, 4);

        builder.Property(static x => x.TaxAmount)
            .HasPrecision(18, 4);

        builder.Property(static x => x.TotalAmount)
            .HasPrecision(18, 4);

        builder.OwnsOne(static x => x.ShippingAddress, navigationBuilder =>
        {
            navigationBuilder.WithOwner();
            navigationBuilder.Property(static x => x.FullName).HasColumnName("shipping_full_name").HasMaxLength(200).IsRequired();
            navigationBuilder.Property(static x => x.PhoneNumber).HasColumnName("shipping_phone_number").HasMaxLength(50).IsRequired();
            navigationBuilder.Property(static x => x.AddressLine1).HasColumnName("shipping_address_line1").HasMaxLength(500).IsRequired();
            navigationBuilder.Property(static x => x.AddressLine2).HasColumnName("shipping_address_line2").HasMaxLength(500);
            navigationBuilder.Property(static x => x.City).HasColumnName("shipping_city").HasMaxLength(200).IsRequired();
            navigationBuilder.Property(static x => x.State).HasColumnName("shipping_state").HasMaxLength(200);
            navigationBuilder.Property(static x => x.PostalCode).HasColumnName("shipping_postal_code").HasMaxLength(20);
            navigationBuilder.Property(static x => x.Country).HasColumnName("shipping_country").HasMaxLength(100).IsRequired();
        });
        builder.Navigation(static x => x.ShippingAddress).IsRequired();

        builder.OwnsOne(static x => x.BillingAddress, navigationBuilder =>
        {
            navigationBuilder.WithOwner();
            navigationBuilder.Property(static x => x.FullName).HasColumnName("billing_full_name").HasMaxLength(200).IsRequired();
            navigationBuilder.Property(static x => x.PhoneNumber).HasColumnName("billing_phone_number").HasMaxLength(50).IsRequired();
            navigationBuilder.Property(static x => x.AddressLine1).HasColumnName("billing_address_line1").HasMaxLength(500).IsRequired();
            navigationBuilder.Property(static x => x.AddressLine2).HasColumnName("billing_address_line2").HasMaxLength(500);
            navigationBuilder.Property(static x => x.City).HasColumnName("billing_city").HasMaxLength(200).IsRequired();
            navigationBuilder.Property(static x => x.State).HasColumnName("billing_state").HasMaxLength(200);
            navigationBuilder.Property(static x => x.PostalCode).HasColumnName("billing_postal_code").HasMaxLength(20);
            navigationBuilder.Property(static x => x.Country).HasColumnName("billing_country").HasMaxLength(100).IsRequired();
        });

        builder.HasMany(static x => x.Items)
            .WithOne(static x => x.Order)
            .HasForeignKey(static x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(static x => x.StatusHistory)
            .WithOne(static x => x.Order)
            .HasForeignKey(static x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ConfigureAuditAndSoftDelete();
    }
}
