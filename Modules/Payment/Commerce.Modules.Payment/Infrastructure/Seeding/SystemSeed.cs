using Commerce.Modules.Payment.Domain;

namespace Commerce.Modules.Payment.Infrastructure.Seeding;

internal static class SystemSeed
{
    internal static readonly PaymentMethod CodPaymentMethod = new()
    {
        Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        Code = "COD",
        Name = "Cash On Delivery",
        IsActive = true,
        CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        CreatedBy = "system",
        IsDeleted = false
    };
}
