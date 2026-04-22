using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Domain;
using Commerce.Modules.Identity.Infrastructure;
using CommerceCore.Application.Abstractions;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Identity.Application.Commands.SyncUserRoles;

public sealed class SyncUserRolesHandler : IRequestHandler<SyncUserRolesCommand, ApiResponse<UserRoleAssignmentsResponse>>
{
    private readonly IdentityDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICurrentUser _currentUser;

    public SyncUserRolesHandler(
        IdentityDbContext dbContext,
        IDateTimeProvider dateTimeProvider,
        ICurrentUser currentUser)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<ApiResponse<UserRoleAssignmentsResponse>> Handle(SyncUserRolesCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var requestedRoleIds = command.RoleIds
            .Distinct()
            .ToList();

        if (requestedRoleIds.Count != command.RoleIds.Count)
        {
            throw new ValidationAppException("Duplicate role ids are not allowed.");
        }

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == command.UserId, cancellationToken)
            ?? throw new NotFoundAppException($"User '{command.UserId}' was not found.");

        var roles = requestedRoleIds.Count == 0
            ? []
            : await _dbContext.Roles
                .Where(x => requestedRoleIds.Contains(x.Id))
                .ToListAsync(cancellationToken);

        if (roles.Count != requestedRoleIds.Count)
        {
            throw new ValidationAppException("One or more roles were not found.");
        }

        if (roles.Any(static x => !x.IsActive))
        {
            throw new ValidationAppException("Inactive roles cannot be assigned.");
        }

        var existingAssignments = await _dbContext.UserRoles
            .IgnoreQueryFilters()
            .Where(x => x.UserId == command.UserId)
            .ToListAsync(cancellationToken);

        var now = _dateTimeProvider.UtcNow;
        var actor = string.IsNullOrWhiteSpace(_currentUser.UserId) ? _currentUser.UserName : _currentUser.UserId;

        foreach (var assignment in existingAssignments)
        {
            if (requestedRoleIds.Contains(assignment.RoleId))
            {
                assignment.IsActive = true;
                assignment.ExpiresAtUtc = null;
                assignment.AssignedAtUtc = now;
                assignment.AssignedBy = actor;
                assignment.IsDeleted = false;
                assignment.DeletedAtUtc = null;
                assignment.DeletedBy = null;
                continue;
            }

            if (!assignment.IsDeleted)
            {
                _dbContext.UserRoles.Remove(assignment);
            }
        }

        var existingRoleIds = existingAssignments
            .Select(static x => x.RoleId)
            .ToHashSet();

        foreach (var roleId in requestedRoleIds)
        {
            if (existingRoleIds.Contains(roleId))
            {
                continue;
            }

            await _dbContext.UserRoles.AddAsync(new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = command.UserId,
                RoleId = roleId,
                AssignedAtUtc = now,
                AssignedBy = actor,
                IsActive = true
            }, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var syncedRoles = await _dbContext.UserRoles
            .Where(x => x.UserId == user.Id)
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
                UserId = user.Id,
                Roles = syncedRoles
            }
        };
    }
}
