using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Infrastructure;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Identity.Application.Queries.GetUserPermissions;

public sealed class GetUserPermissionsHandler : IRequestHandler<GetUserPermissionsQuery, ApiResponse<UserPermissionAssignmentsResponse>>
{
    private readonly IdentityDbContext _dbContext;

    public GetUserPermissionsHandler(IdentityDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<ApiResponse<UserPermissionAssignmentsResponse>> Handle(GetUserPermissionsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var userExists = await _dbContext.Users
            .AnyAsync(x => x.Id == query.UserId, cancellationToken);
        if (!userExists)
        {
            throw new NotFoundAppException($"User '{query.UserId}' was not found.");
        }

        var permissions = await _dbContext.UserPermissions
            .Where(x => x.UserId == query.UserId)
            .Include(x => x.Permission)
            .OrderBy(x => x.Permission.SortOrder)
            .ThenBy(x => x.Permission.Code)
            .Select(x => new UserPermissionAssignmentDto
            {
                PermissionId = x.PermissionId,
                PermissionCode = x.Permission.Code,
                PermissionName = x.Permission.Name,
                Module = x.Permission.Module,
                Feature = x.Permission.Feature,
                GroupName = x.Permission.GroupName,
                Effect = x.Effect.ToString(),
                AssignedAtUtc = x.AssignedAtUtc,
                AssignedBy = x.AssignedBy,
                ExpiresAtUtc = x.ExpiresAtUtc
            })
            .ToListAsync(cancellationToken);

        return new ApiResponse<UserPermissionAssignmentsResponse>
        {
            Status = StatusCodes.Status200OK,
            Data = new UserPermissionAssignmentsResponse
            {
                UserId = query.UserId,
                Permissions = permissions
            }
        };
    }
}
