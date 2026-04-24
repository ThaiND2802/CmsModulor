using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Infrastructure;
using Commerce.Modules.Identity.Infrastructure.Authentication;
using CommerceCore.Application.Abstractions;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Identity.Application.Commands.ResetUserPassword;

public sealed class ResetUserPasswordHandler : IRequestHandler<ResetUserPasswordCommand, ApiResponse<ResetUserPasswordResponse>>
{
    private readonly IdentityDbContext _dbContext;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ResetUserPasswordHandler(
        IdentityDbContext dbContext,
        IPasswordHasherService passwordHasherService,
        IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _passwordHasherService = passwordHasherService ?? throw new ArgumentNullException(nameof(passwordHasherService));
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    public async Task<ApiResponse<ResetUserPasswordResponse>> Handle(ResetUserPasswordCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken)
            ?? throw new NotFoundAppException($"User '{command.Id}' was not found.");

        if (user.IsSystem)
        {
            throw new ForbiddenAppException($"System user '{command.Id}' cannot be modified.");
        }

        if (!string.IsNullOrWhiteSpace(user.PasswordHash)
            && _passwordHasherService.Verify(user.PasswordHash, command.NewPassword))
        {
            throw new ValidationAppException("New password must be different from the current password.");
        }

        user.PasswordHash = _passwordHasherService.Hash(command.NewPassword);

        await RevokeSessionsAsync(user.Id, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResponse<ResetUserPasswordResponse>
        {
            Status = StatusCodes.Status200OK,
            Data = new ResetUserPasswordResponse
            {
                Id = user.Id,
                PasswordReset = true
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
