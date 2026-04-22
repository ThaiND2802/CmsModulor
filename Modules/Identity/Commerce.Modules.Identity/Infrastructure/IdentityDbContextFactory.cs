using CommerceCore.Application.Abstractions;
using CommerceCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Commerce.Modules.Identity.Infrastructure;

public sealed class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>();
        var connectionString = ResolveConnectionString(args);
        optionsBuilder.UseDefaultDatabase(connectionString);

        return new IdentityDbContext(optionsBuilder.Options, new DesignTimeDateTimeProvider(), new DesignTimeCurrentUser());
    }

    private static string ResolveConnectionString(string[] args)
    {
        const string prefix = "--connection=";

        foreach (var arg in args)
        {
            if (arg.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                var value = arg[prefix.Length..].Trim();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }
        }

        var environmentConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default");
        if (!string.IsNullOrWhiteSpace(environmentConnectionString))
        {
            return environmentConnectionString;
        }

        throw new InvalidOperationException("ConnectionStrings__Default is required for design-time DbContext creation.");
    }

    private sealed class DesignTimeDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }

    private sealed class DesignTimeCurrentUser : ICurrentUser
    {
        public string? UserId => "system";

        public string? UserName => "system";

        public bool IsAuthenticated => true;
    }
}
