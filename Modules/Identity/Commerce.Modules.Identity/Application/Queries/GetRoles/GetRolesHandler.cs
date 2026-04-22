using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Application.Mappings;
using Commerce.Modules.Identity.Infrastructure;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Identity.Application.Queries.GetRoles;

public sealed class GetRolesHandler : IRequestHandler<GetRolesQuery, PagedApiResponse<RoleDto>>
{
    private readonly IdentityDbContext _dbContext;

    public GetRolesHandler(IdentityDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<PagedApiResponse<RoleDto>> Handle(GetRolesQuery query, CancellationToken cancellationToken)
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

        var rolesQuery = _dbContext.Roles
            .OrderBy(static x => x.Code);
        var total = await rolesQuery.CountAsync(cancellationToken);
        var page = query.NormalizedPage;
        var pageSize = query.NormalizedPageSize;
        var roles = await rolesQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedApiResponse<RoleDto>
        {
            Status = StatusCodes.Status200OK,
            Data = roles.Select(static x => x.ToRoleDto()).ToList(),
            Pagination = new PaginationMetadata
            {
                Total = total,
                Page = page,
                PageSize = pageSize
            }
        };
    }
}
