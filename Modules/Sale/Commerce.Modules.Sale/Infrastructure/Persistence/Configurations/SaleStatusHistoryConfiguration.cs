using Commerce.Modules.Sale.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Commerce.Modules.Sale.Infrastructure.Persistence.Configurations;

public sealed class SaleStatusHistoryConfiguration : IEntityTypeConfiguration<SaleStatusHistory>
{
    public void Configure(EntityTypeBuilder<SaleStatusHistory> builder)
    {
        builder.HasKey(static x => x.Id);

        builder.Property(static x => x.FromStatus)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(static x => x.ToStatus)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(static x => x.Note)
            .HasMaxLength(1000);

        builder.Property(static x => x.ChangedBy)
            .HasMaxLength(100);
    }
}
