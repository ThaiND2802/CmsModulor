using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Application.Mappings;
using Commerce.Modules.Identity.Infrastructure;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Identity.Application.Queries.GetUsers;

public sealed class GetUsersHandler : IRequestHandler<GetUsersQuery, PagedApiResponse<UserDto>>
{
    private readonly IdentityDbContext _dbContext;

    public GetUsersHandler(IdentityDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<PagedApiResponse<UserDto>> Handle(GetUsersQuery query, CancellationToken cancellationToken)
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

        var usersQuery = _dbContext.Users
            .OrderBy(static x => x.UserName);
        var total = await usersQuery.CountAsync(cancellationToken);
        var page = query.NormalizedPage;
        var pageSize = query.NormalizedPageSize;
        var users = await usersQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedApiResponse<UserDto>
        {
            Status = StatusCodes.Status200OK,
            Data = users.Select(static x => x.ToUserDto()).ToList(),
            Pagination = new PaginationMetadata
            {
                Total = total,
                Page = page,
                PageSize = pageSize
            }
        };
    }
}
