using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Domain;
using Commerce.Modules.Identity.Infrastructure;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Identity.Application.Commands.DeleteRole;

public sealed class DeleteRoleHandler : IRequestHandler<DeleteRoleCommand, ApiResponse<DeleteRoleResponse>>
{
    private readonly IdentityDbContext _dbContext;

    public DeleteRoleHandler(IdentityDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<ApiResponse<DeleteRoleResponse>> Handle(DeleteRoleCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var role = await _dbContext.Roles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

        if (role is null || role.IsDeleted)
        {
            throw new NotFoundAppException($"Role '{command.Id}' was not found.");
        }

        if (role.IsSystem)
        {
            throw new ForbiddenAppException($"System role '{command.Id}' cannot be deleted.");
        }

        _dbContext.Roles.Remove(role);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResponse<DeleteRoleResponse>
        {
            Status = 200,
            Data = new DeleteRoleResponse
            {
                Id = role.Id,
                Deleted = true
            }
        };
    }
}
