using CommerceCore.Application.Abstractions;
using CommerceCore.Infrastructure.Persistence;
using Commerce.Modules.Payment.Domain;
using Commerce.Modules.Payment.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Payment.Infrastructure;

public sealed class PaymentDbContext : BaseDbContext
{
    public PaymentDbContext(
        DbContextOptions<PaymentDbContext> options,
        IDateTimeProvider dateTimeProvider,
        ICurrentUser currentUser)
        : base(options, dateTimeProvider, currentUser)
    {
    }

    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyPaymentSystemSeed();
    }

    public Task EnsureSeedDataAsync(CancellationToken cancellationToken = default)
    {
        return Database.EnsureCreatedAsync(cancellationToken);
    }
}
