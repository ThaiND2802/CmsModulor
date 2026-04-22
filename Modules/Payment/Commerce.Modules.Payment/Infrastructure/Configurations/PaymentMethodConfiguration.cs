using CommerceCore.Infrastructure.Persistence;
using Commerce.Modules.Payment.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Commerce.Modules.Payment.Infrastructure.Configurations;

public sealed class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.HasKey(static x => x.Id);

        builder.Property(static x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(static x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(static x => x.IsActive);

        builder.HasIndex(static x => x.Code)
            .IsUnique();

        builder.ConfigureAuditAndSoftDelete();
    }
}
