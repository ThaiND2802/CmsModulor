using CommerceCore.Application.Abstractions;
using CommerceCore.Infrastructure.Persistence;
using Commerce.Modules.Payment.Domain;
using Commerce.Modules.Payment.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

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

    public async Task EnsureSeedDataAsync(CancellationToken cancellationToken = default)
    {
        var databaseCreator = Database.GetService<IRelationalDatabaseCreator>();

        if (!await databaseCreator.ExistsAsync(cancellationToken))
        {
            await Database.EnsureCreatedAsync(cancellationToken);
            return;
        }

        if (await PaymentMethodTableExistsAsync(cancellationToken))
        {
            return;
        }

        await databaseCreator.CreateTablesAsync(cancellationToken);
    }

    private async Task<bool> PaymentMethodTableExistsAsync(CancellationToken cancellationToken)
    {
        await Database.OpenConnectionAsync(cancellationToken);

        try
        {
            await using var command = Database.GetDbConnection().CreateCommand();
            command.CommandText = """
                select exists (
                    select 1
                    from information_schema.tables
                    where table_schema = 'public'
                      and table_name = 'payment_methods'
                )
                """;

            var result = await command.ExecuteScalarAsync(cancellationToken);
            return result is true;
        }
        finally
        {
            await Database.CloseConnectionAsync();
        }
    }
}
