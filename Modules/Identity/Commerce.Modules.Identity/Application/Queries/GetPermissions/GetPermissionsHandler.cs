using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Application.Mappings;
using Commerce.Modules.Identity.Infrastructure;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Identity.Application.Queries.GetPermissions;

public sealed class GetPermissionsHandler : IRequestHandler<GetPermissionsQuery, PagedApiResponse<PermissionDto>>
{
    private readonly IdentityDbContext _dbContext;

    public GetPermissionsHandler(IdentityDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<PagedApiResponse<PermissionDto>> Handle(GetPermissionsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.Page <= 0)
        {
            throw new ValidationAppException("Page must be greater than 0.");
        }

        if (query.PageSize <= 0)
        {
            throw new ValidationAppException("Page size must be greater than 0.");
        }

        var permissionsQuery = _dbContext.Permissions
            .OrderBy(static x => x.SortOrder)
            .ThenBy(static x => x.Code);
        var total = await permissionsQuery.CountAsync(cancellationToken);
        var page = query.NormalizedPage;
        var pageSize = query.NormalizedPageSize;
        var permissions = await permissionsQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedApiResponse<PermissionDto>
        {
            Status = StatusCodes.Status200OK,
            Data = permissions.Select(static x => x.ToPermissionDto()).ToList(),
            Pagination = new PaginationMetadata
            {
                Total = total,
                Page = page,
                PageSize = pageSize
            }
        };
    }
}
