using Commerce.Modules.Catalog.Application.Mappings;
using Commerce.Modules.Catalog.Infrastructure;
using CommerceCore.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Commerce.Modules.Catalog.Tests.Common;

internal static class CatalogTestDbContextFactory
{
    public static CatalogDbContext CreateInMemory(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString("N"))
            .Options;

        var dateTimeProvider = Substitute.For<IDateTimeProvider>();
        dateTimeProvider.UtcNow.Returns(new DateTime(2026, 4, 24, 0, 0, 0, DateTimeKind.Utc));

        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.UserId.Returns("test-user");
        currentUser.UserName.Returns("tester");
        currentUser.IsAuthenticated.Returns(true);

        return new CatalogDbContext(options, dateTimeProvider, currentUser);
    }
}
