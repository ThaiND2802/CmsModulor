using Microsoft.EntityFrameworkCore;

namespace CommerceCore.Infrastructure.Persistence;

public static class DbContextOptionsBuilderExtensions
{
    public static DbContextOptionsBuilder UseDefaultDatabase(
        this DbContextOptionsBuilder options,
        string connectionString)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        return options.UseNpgsql(connectionString);
    }

    public static DbContextOptionsBuilder<TContext> UseDefaultDatabase<TContext>(
        this DbContextOptionsBuilder<TContext> options,
        string connectionString)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        return (DbContextOptionsBuilder<TContext>)options.UseNpgsql(connectionString);
    }
}
