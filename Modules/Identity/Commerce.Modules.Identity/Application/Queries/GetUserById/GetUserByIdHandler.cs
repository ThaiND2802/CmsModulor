using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Application.Mappings;
using Commerce.Modules.Identity.Infrastructure;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Identity.Application.Queries.GetUserById;

public sealed class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, ApiResponse<UserDto>>
{
    private readonly IdentityDbContext _dbContext;

    public GetUserByIdHandler(IdentityDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<ApiResponse<UserDto>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken)
            ?? throw new NotFoundAppException($"User '{query.Id}' was not found.");

        return new ApiResponse<UserDto>
        {
            Status = StatusCodes.Status200OK,
            Data = user.ToUserDto()
        };
    }
}
