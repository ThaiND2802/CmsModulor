using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Infrastructure;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Identity.Application.Queries.GetRolePermissions;

public sealed class GetRolePermissionsHandler : IRequestHandler<GetRolePermissionsQuery, ApiResponse<RolePermissionAssignmentsResponse>>
{
    private readonly IdentityDbContext _dbContext;

    public GetRolePermissionsHandler(IdentityDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<ApiResponse<RolePermissionAssignmentsResponse>> Handle(GetRolePermissionsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var roleExists = await _dbContext.Roles
            .AnyAsync(x => x.Id == query.RoleId, cancellationToken);
        if (!roleExists)
        {
            throw new NotFoundAppException($"Role '{query.RoleId}' was not found.");
        }

        var permissions = await _dbContext.RolePermissions
            .Where(x => x.RoleId == query.RoleId)
            .Include(x => x.Permission)
            .OrderBy(x => x.Permission.SortOrder)
            .ThenBy(x => x.Permission.Code)
            .Select(x => new RolePermissionAssignmentDto
            {
                PermissionId = x.PermissionId,
                PermissionCode = x.Permission.Code,
                PermissionName = x.Permission.Name,
                Module = x.Permission.Module,
                Feature = x.Permission.Feature,
                GroupName = x.Permission.GroupName,
                Effect = x.Effect.ToString(),
                AssignedAtUtc = x.AssignedAtUtc,
                AssignedBy = x.AssignedBy
            })
            .ToListAsync(cancellationToken);

        return new ApiResponse<RolePermissionAssignmentsResponse>
        {
            Status = StatusCodes.Status200OK,
            Data = new RolePermissionAssignmentsResponse
            {
                RoleId = query.RoleId,
                Permissions = permissions
            }
        };
    }
}
