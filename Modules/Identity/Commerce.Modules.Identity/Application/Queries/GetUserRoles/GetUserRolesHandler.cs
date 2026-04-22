using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Infrastructure;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Identity.Application.Queries.GetUserRoles;

public sealed class GetUserRolesHandler : IRequestHandler<GetUserRolesQuery, ApiResponse<UserRoleAssignmentsResponse>>
{
    private readonly IdentityDbContext _dbContext;

    public GetUserRolesHandler(IdentityDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<ApiResponse<UserRoleAssignmentsResponse>> Handle(GetUserRolesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var userExists = await _dbContext.Users
            .AnyAsync(x => x.Id == query.UserId, cancellationToken);
        if (!userExists)
        {
            throw new NotFoundAppException($"User '{query.UserId}' was not found.");
        }

        var roles = await _dbContext.UserRoles
            .Where(x => x.UserId == query.UserId)
            .Include(x => x.Role)
            .OrderBy(x => x.Role.Code)
            .Select(x => new UserRoleAssignmentDto
            {
                RoleId = x.RoleId,
                RoleCode = x.Role.Code,
                RoleName = x.Role.Name,
                IsActive = x.IsActive,
                AssignedAtUtc = x.AssignedAtUtc,
                AssignedBy = x.AssignedBy,
                ExpiresAtUtc = x.ExpiresAtUtc
            })
            .ToListAsync(cancellationToken);

        return new ApiResponse<UserRoleAssignmentsResponse>
        {
            Status = StatusCodes.Status200OK,
            Data = new UserRoleAssignmentsResponse
            {
                UserId = query.UserId,
                Roles = roles
            }
        };
    }
}
