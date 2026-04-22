using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Application.Mappings;
using Commerce.Modules.Identity.Infrastructure;
using CommerceCore.SharedKernel.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Identity.Application.Queries.GetUsers;

public sealed class GetUsersHandler
{
    private readonly IdentityDbContext _dbContext;

    public GetUsersHandler(IdentityDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<GetUsersResult> HandleAsync(GetUsersQuery query, CancellationToken cancellationToken)
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
        var users = await usersQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new GetUsersResult(
            users.Select(static x => x.ToUserDto()).ToList(),
            total,
            query.Page,
            query.PageSize);
    }
}
