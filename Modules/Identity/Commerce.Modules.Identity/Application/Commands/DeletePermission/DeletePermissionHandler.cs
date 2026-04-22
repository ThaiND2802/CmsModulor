using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Domain;
using Commerce.Modules.Identity.Infrastructure;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Identity.Application.Commands.DeletePermission;

public sealed class DeletePermissionHandler : IRequestHandler<DeletePermissionCommand, ApiResponse<DeletePermissionResponse>>
{
    private readonly IdentityDbContext _dbContext;

    public DeletePermissionHandler(IdentityDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<ApiResponse<DeletePermissionResponse>> Handle(DeletePermissionCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var permission = await _dbContext.Permissions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

        if (permission is null || permission.IsDeleted)
        {
            throw new NotFoundAppException($"Permission '{command.Id}' was not found.");
        }

        if (permission.IsSystem)
        {
            throw new ForbiddenAppException($"System permission '{command.Id}' cannot be deleted.");
        }

        _dbContext.Permissions.Remove(permission);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResponse<DeletePermissionResponse>
        {
            Status = 200,
            Data = new DeletePermissionResponse
            {
                Id = permission.Id,
                Deleted = true
            }
        };
    }
}
