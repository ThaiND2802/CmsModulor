using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Application.Mappings;
using Commerce.Modules.Identity.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Identity.Application.Queries.GetAuthorizationOverview;

public sealed class GetAuthorizationOverviewHandler
{
    private static readonly IReadOnlyCollection<string> ResolutionOrder =
    [
        "User direct deny",
        "User direct allow",
        "Role deny",
        "Role allow",
        "Default deny"
    ];

    private readonly IdentityDbContext _dbContext;

    public GetAuthorizationOverviewHandler(IdentityDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<AuthorizationOverviewDto> HandleAsync(
        GetAuthorizationOverviewQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var roles = await _dbContext.Roles
            .OrderBy(static x => x.Code)
            .ToListAsync(cancellationToken);

        var permissions = await _dbContext.Permissions
            .OrderBy(static x => x.SortOrder)
            .ThenBy(static x => x.Code)
            .ToListAsync(cancellationToken);

        return new AuthorizationOverviewDto(
            true,
            true,
            true,
            "IdentityDbContext",
            "X-Commerce-UserId -> User.NormalizedUserName",
            ResolutionOrder,
            roles.Select(static x => x.ToRoleOverviewDto()).ToList(),
            permissions.Select(static x => x.ToPermissionOverviewDto()).ToList());
    }
}
