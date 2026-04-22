using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Application.Mappings;
using Commerce.Modules.Identity.Domain;
using Commerce.Modules.Identity.Infrastructure;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Identity.Application.Commands.UpdatePermission;

public sealed class UpdatePermissionHandler : IRequestHandler<UpdatePermissionCommand, ApiResponse<PermissionDto>>
{
    private readonly IdentityDbContext _dbContext;

    public UpdatePermissionHandler(IdentityDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<ApiResponse<PermissionDto>> Handle(UpdatePermissionCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            throw new ValidationAppException("Permission name is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Module))
        {
            throw new ValidationAppException("Permission module is required.");
        }

        if (string.IsNullOrWhiteSpace(command.GroupName))
        {
            throw new ValidationAppException("Permission group name is required.");
        }

        var permission = await _dbContext.Permissions
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken)
            ?? throw new NotFoundAppException($"Permission '{command.Id}' was not found.");

        if (permission.IsSystem)
        {
            throw new ForbiddenAppException($"System permission '{command.Id}' cannot be modified.");
        }

        permission.Name = command.Name.Trim();
        permission.Description = string.IsNullOrWhiteSpace(command.Description) ? null : command.Description.Trim();
        permission.Module = command.Module.Trim();
        permission.Feature = string.IsNullOrWhiteSpace(command.Feature) ? null : command.Feature.Trim();
        permission.GroupName = command.GroupName.Trim();
        permission.SortOrder = command.SortOrder;
        permission.IsActive = command.IsActive;

        _dbContext.Permissions.Update(permission);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResponse<PermissionDto>
        {
            Status = StatusCodes.Status200OK,
            Data = permission.ToPermissionDto()
        };
    }
}
