using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Infrastructure;
using Commerce.Modules.Identity.Infrastructure.Authentication;
using CommerceCore.Application.Abstractions;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Identity.Application.Commands.ChangePassword;

public sealed class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, ApiResponse<ChangePasswordResponse>>
{
    private readonly IdentityDbContext _dbContext;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly ICurrentUser _currentUser;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ChangePasswordHandler(
        IdentityDbContext dbContext,
        IPasswordHasherService passwordHasherService,
        ICurrentUser currentUser,
        IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _passwordHasherService = passwordHasherService ?? throw new ArgumentNullException(nameof(passwordHasherService));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    public async Task<ApiResponse<ChangePasswordResponse>> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (!_currentUser.IsAuthenticated || !Guid.TryParse(_currentUser.UserId, out var userId))
        {
            throw new UnauthorizedAppException("The current user is not authenticated.");
        }

        if (string.IsNullOrWhiteSpace(command.CurrentPassword))
        {
            throw new ValidationAppException("Current password is required.");
        }

        if (string.IsNullOrWhiteSpace(command.NewPassword))
        {
            throw new ValidationAppException("New password is required.");
        }

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken)
            ?? throw new NotFoundAppException($"User '{userId}' was not found.");

        if (string.IsNullOrWhiteSpace(user.PasswordHash)
            || !_passwordHasherService.Verify(user.PasswordHash, command.CurrentPassword))
        {
            throw new UnauthorizedAppException("The current password is invalid.");
        }

        if (_passwordHasherService.Verify(user.PasswordHash, command.NewPassword))
        {
            throw new ValidationAppException("New password must be different from the current password.");
        }

        user.PasswordHash = _passwordHasherService.Hash(command.NewPassword);

        await RevokeSessionsAsync(userId, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResponse<ChangePasswordResponse>
        {
            Status = StatusCodes.Status200OK,
            Data = new ChangePasswordResponse
            {
                Changed = true
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
