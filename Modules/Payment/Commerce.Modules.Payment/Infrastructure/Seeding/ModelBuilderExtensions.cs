using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Payment.Infrastructure.Seeding;

internal static class ModelBuilderExtensions
{
    internal static void ApplyPaymentSystemSeed(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.Entity<Domain.PaymentMethod>()
            .HasData(SystemSeed.CodPaymentMethod);
    }
}
