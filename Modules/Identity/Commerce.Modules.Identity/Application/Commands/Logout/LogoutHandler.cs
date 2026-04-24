using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Infrastructure;
using Commerce.Modules.Identity.Infrastructure.Authentication;
using CommerceCore.Application.Abstractions;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Identity.Application.Commands.Logout;

public sealed class LogoutHandler : IRequestHandler<LogoutCommand, ApiResponse<LogoutResponse>>
{
    private readonly IdentityDbContext _dbContext;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public LogoutHandler(
        IdentityDbContext dbContext,
        IRefreshTokenService refreshTokenService,
        IDateTimeProvider dateTimeProvider)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _refreshTokenService = refreshTokenService ?? throw new ArgumentNullException(nameof(refreshTokenService));
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    public async Task<ApiResponse<LogoutResponse>> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            throw new ValidationAppException("Refresh token is required.");
        }

        var refreshTokenHash = _refreshTokenService.HashToken(command.RefreshToken);
        var session = await _dbContext.UserSessions
            .FirstOrDefaultAsync(x => x.RefreshTokenHash == refreshTokenHash, cancellationToken);

        if (session is not null && !session.RevokedAtUtc.HasValue)
        {
            session.RevokedAtUtc = _dateTimeProvider.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return new ApiResponse<LogoutResponse>
        {
            Status = StatusCodes.Status200OK,
            Data = new LogoutResponse
            {
                LoggedOut = true
            }
        };
    }
}
