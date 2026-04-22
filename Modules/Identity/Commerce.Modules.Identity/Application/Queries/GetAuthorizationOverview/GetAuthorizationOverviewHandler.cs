using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Application.Mappings;
using Commerce.Modules.Identity.Infrastructure;
using CommerceCore.Application.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Identity.Application.Queries.GetAuthorizationOverview;

public sealed class GetAuthorizationOverviewHandler
    : IRequestHandler<GetAuthorizationOverviewQuery, ApiResponse<AuthorizationOverviewDto>>
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

    public async Task<ApiResponse<AuthorizationOverviewDto>> Handle(
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

        return new ApiResponse<AuthorizationOverviewDto>
        {
            Status = StatusCodes.Status200OK,
            Data = new AuthorizationOverviewDto
            {
                ModuleGatingRunsFirst = true,
                FeatureGatingRunsBeforeAuthorization = true,
                DbPermissionGateImplemented = true,
                PermissionSource = "IdentityDbContext",
                CurrentDevelopmentUserLookup = "X-Commerce-UserId -> User.Id (GUID)",
                ResolutionOrder = ResolutionOrder,
                Roles = roles.Select(static x => x.ToRoleOverviewDto()).ToList(),
                Permissions = permissions.Select(static x => x.ToPermissionOverviewDto()).ToList()
            }
        };
    }
}
