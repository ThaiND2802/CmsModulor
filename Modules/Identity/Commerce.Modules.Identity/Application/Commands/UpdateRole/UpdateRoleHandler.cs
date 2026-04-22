using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Application.Mappings;
using Commerce.Modules.Identity.Domain;
using Commerce.Modules.Identity.Infrastructure;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Identity.Application.Commands.UpdateRole;

public sealed class UpdateRoleHandler : IRequestHandler<UpdateRoleCommand, ApiResponse<RoleDto>>
{
    private readonly IdentityDbContext _dbContext;

    public UpdateRoleHandler(IdentityDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<ApiResponse<RoleDto>> Handle(UpdateRoleCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            throw new ValidationAppException("Role name is required.");
        }

        var role = await _dbContext.Roles
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken)
            ?? throw new NotFoundAppException($"Role '{command.Id}' was not found.");

        if (role.IsSystem)
        {
            throw new ForbiddenAppException($"System role '{command.Id}' cannot be modified.");
        }

        role.Name = command.Name.Trim();
        role.Description = string.IsNullOrWhiteSpace(command.Description) ? null : command.Description.Trim();
        role.IsActive = command.IsActive;

        _dbContext.Roles.Update(role);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResponse<RoleDto>
        {
            Status = StatusCodes.Status200OK,
            Data = role.ToRoleDto()
        };
    }
}
