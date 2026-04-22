using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Domain;
using Commerce.Modules.Identity.Infrastructure;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Identity.Application.Commands.DeleteUser;

public sealed class DeleteUserHandler : IRequestHandler<DeleteUserCommand, ApiResponse<DeleteUserResponse>>
{
    private readonly IdentityDbContext _dbContext;

    public DeleteUserHandler(IdentityDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<ApiResponse<DeleteUserResponse>> Handle(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var user = await _dbContext.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

        if (user is null || user.IsDeleted)
        {
            throw new NotFoundAppException($"User '{command.Id}' was not found.");
        }

        if (user.IsSystem)
        {
            throw new ForbiddenAppException($"System user '{command.Id}' cannot be deleted.");
        }

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResponse<DeleteUserResponse>
        {
            Status = 200,
            Data = new DeleteUserResponse
            {
                Id = user.Id,
                Deleted = true
            }
        };
    }
}
