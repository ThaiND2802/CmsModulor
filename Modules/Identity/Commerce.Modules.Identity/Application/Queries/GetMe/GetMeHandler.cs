using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Application.Mappings;
using Commerce.Modules.Identity.Infrastructure;
using CommerceCore.Application.Abstractions;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Identity.Application.Queries.GetMe;

public sealed class GetMeHandler : IRequestHandler<GetMeQuery, ApiResponse<UserDto>>
{
    private readonly IdentityDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public GetMeHandler(IdentityDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<ApiResponse<UserDto>> Handle(GetMeQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (!_currentUser.IsAuthenticated || !Guid.TryParse(_currentUser.UserId, out var userId))
        {
            throw new UnauthorizedAppException("The current user is not authenticated.");
        }

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken)
            ?? throw new NotFoundAppException("The current user was not found.");

        return new ApiResponse<UserDto>
        {
            Status = StatusCodes.Status200OK,
            Data = user.ToUserDto()
        };
    }
}
