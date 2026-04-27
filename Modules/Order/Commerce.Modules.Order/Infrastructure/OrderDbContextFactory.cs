using CommerceCore.Application.Abstractions;
using CommerceCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Commerce.Modules.Order.Infrastructure;

public sealed class OrderDbContextFactory : IDesignTimeDbContextFactory<OrderDbContext>
{
    public OrderDbContext CreateDbContext(string[] args)
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var hostPath = Path.GetFullPath(Path.Combine(currentDirectory, "Host/WebApi/Commerce.Host.WebApi"));
        var basePath = Directory.Exists(hostPath) ? hostPath : currentDirectory;

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        if (!Directory.Exists(basePath))
        {
            throw new InvalidOperationException($"Unable to locate design-time configuration path: {basePath}");
        }

        var connectionString = configuration.GetConnectionString("Default")
            ?? configuration["ConnectionStrings:Default"]
            ?? configuration["ConnectionStrings__Default"]
            ?? throw new InvalidOperationException("ConnectionStrings:Default is missing for design-time OrderDbContext creation.");

        var optionsBuilder = new DbContextOptionsBuilder<OrderDbContext>();
        optionsBuilder.UseDefaultDatabase(connectionString);

        return new OrderDbContext(optionsBuilder.Options, new DesignTimeDateTimeProvider(), new DesignTimeCurrentUser());
    }

    private sealed class DesignTimeDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }

    private sealed class DesignTimeCurrentUser : ICurrentUser
    {
        public string? UserId => "design-time";
        public string? UserName => "design-time";
        public bool IsAuthenticated => false;
    }
}
