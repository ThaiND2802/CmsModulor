using Commerce.Modules.Sale.Domain;
using CommerceCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaleEntity = Commerce.Modules.Sale.Domain.Sale;

namespace Commerce.Modules.Sale.Infrastructure.Persistence.Configurations;

public sealed class SaleConfiguration : IEntityTypeConfiguration<SaleEntity>
{
    public void Configure(EntityTypeBuilder<SaleEntity> builder)
    {
        builder.HasKey(static x => x.Id);

        builder.Property(static x => x.SaleNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(static x => x.SaleNumber)
            .IsUnique();

        builder.Property(static x => x.CustomerEmail)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(static x => x.CustomerPhone)
            .HasMaxLength(50);

        builder.Property(static x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(static x => x.Channel)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(static x => x.PaymentMethod)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(static x => x.PaymentStatus)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(static x => x.PaidAmount)
            .HasPrecision(18, 4);

        builder.Property(static x => x.PaymentReference)
            .HasMaxLength(200);

        builder.Property(static x => x.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(static x => x.Notes)
            .HasMaxLength(2000);

        builder.Property(static x => x.OrderId);

        builder.Property(static x => x.ExpiresAtUtc);

        builder.Property(static x => x.CouponCode)
            .HasMaxLength(100);

        builder.Property(static x => x.CouponType)
            .HasMaxLength(50);

        builder.Property(static x => x.CouponValue)
            .HasPrecision(18, 4);

        builder.Property(static x => x.SubtotalAmount).HasPrecision(18, 4);
        builder.Property(static x => x.DiscountAmount).HasPrecision(18, 4);
        builder.Property(static x => x.BaseDiscountAmount).HasPrecision(18, 4);
        builder.Property(static x => x.CouponDiscountAmount).HasPrecision(18, 4);
        builder.Property(static x => x.ShippingAmount).HasPrecision(18, 4);
        builder.Property(static x => x.BaseShippingAmount).HasPrecision(18, 4);
        builder.Property(static x => x.CouponShippingDiscountAmount).HasPrecision(18, 4);
        builder.Property(static x => x.TaxAmount).HasPrecision(18, 4);
        builder.Property(static x => x.TotalAmount).HasPrecision(18, 4);

        builder.OwnsOne(static x => x.ShippingAddress, navigationBuilder =>
        {
            navigationBuilder.WithOwner();
            navigationBuilder.Property(static x => x.FullName).HasColumnName("shipping_full_name").HasMaxLength(200);
            navigationBuilder.Property(static x => x.PhoneNumber).HasColumnName("shipping_phone_number").HasMaxLength(50);
            navigationBuilder.Property(static x => x.AddressLine1).HasColumnName("shipping_address_line1").HasMaxLength(500);
            navigationBuilder.Property(static x => x.AddressLine2).HasColumnName("shipping_address_line2").HasMaxLength(500);
            navigationBuilder.Property(static x => x.City).HasColumnName("shipping_city").HasMaxLength(200);
            navigationBuilder.Property(static x => x.State).HasColumnName("shipping_state").HasMaxLength(200);
            navigationBuilder.Property(static x => x.PostalCode).HasColumnName("shipping_postal_code").HasMaxLength(20);
            navigationBuilder.Property(static x => x.Country).HasColumnName("shipping_country").HasMaxLength(100);
        });

        builder.OwnsOne(static x => x.BillingAddress, navigationBuilder =>
        {
            navigationBuilder.WithOwner();
            navigationBuilder.Property(static x => x.FullName).HasColumnName("billing_full_name").HasMaxLength(200);
            navigationBuilder.Property(static x => x.PhoneNumber).HasColumnName("billing_phone_number").HasMaxLength(50);
            navigationBuilder.Property(static x => x.AddressLine1).HasColumnName("billing_address_line1").HasMaxLength(500);
            navigationBuilder.Property(static x => x.AddressLine2).HasColumnName("billing_address_line2").HasMaxLength(500);
            navigationBuilder.Property(static x => x.City).HasColumnName("billing_city").HasMaxLength(200);
            navigationBuilder.Property(static x => x.State).HasColumnName("billing_state").HasMaxLength(200);
            navigationBuilder.Property(static x => x.PostalCode).HasColumnName("billing_postal_code").HasMaxLength(20);
            navigationBuilder.Property(static x => x.Country).HasColumnName("billing_country").HasMaxLength(100);
        });

        builder.HasMany(static x => x.Items)
            .WithOne(static x => x.Sale)
            .HasForeignKey(static x => x.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(static x => x.StatusHistory)
            .WithOne(static x => x.Sale)
            .HasForeignKey(static x => x.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ConfigureAuditAndSoftDelete();
    }
}
