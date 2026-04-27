using CommerceCore.Application.Abstractions;
using CommerceCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Commerce.Modules.Inventory.Infrastructure;

public sealed class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    public InventoryDbContext CreateDbContext(string[] args)
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var basePath = ResolveBasePath(currentDirectory);

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
            ?? throw new InvalidOperationException("ConnectionStrings:Default is missing for design-time InventoryDbContext creation.");

        var optionsBuilder = new DbContextOptionsBuilder<InventoryDbContext>();
        optionsBuilder.UseDefaultDatabase(connectionString);

        return new InventoryDbContext(optionsBuilder.Options, new DesignTimeDateTimeProvider(), new DesignTimeCurrentUser());
    }

    private static string ResolveBasePath(string currentDirectory)
    {
        var directory = new DirectoryInfo(currentDirectory);
        while (directory is not null)
        {
            var hostPath = Path.Combine(directory.FullName, "Host", "WebApi", "Commerce.Host.WebApi");
            if (Directory.Exists(hostPath))
            {
                return hostPath;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException($"Unable to locate design-time configuration path from: {currentDirectory}");
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
