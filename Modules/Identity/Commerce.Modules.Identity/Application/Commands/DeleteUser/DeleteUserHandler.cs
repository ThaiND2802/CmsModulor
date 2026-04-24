using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Domain;
using Commerce.Modules.Identity.Infrastructure;
using CommerceCore.Application.Abstractions;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Identity.Application.Commands.DeleteUser;

public sealed class DeleteUserHandler : IRequestHandler<DeleteUserCommand, ApiResponse<DeleteUserResponse>>
{
    private readonly IdentityDbContext _dbContext;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DeleteUserHandler(IdentityDbContext dbContext, IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
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

        user.IsActive = false;
        await RevokeSessionsAsync(user.Id, cancellationToken);
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

    private async Task RevokeSessionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var now = _dateTimeProvider.UtcNow;
        var sessions = await _dbContext.UserSessions
            .Where(x => x.UserId == userId && !x.RevokedAtUtc.HasValue)
            .ToListAsync(cancellationToken);

        foreach (var session in sessions)
        {
            session.RevokedAtUtc = now;
            session.LastUsedAtUtc = now;
        }
    }
}
