using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Application.Mappings;
using Commerce.Modules.Identity.Infrastructure;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Identity.Application.Queries.GetRoleById;

public sealed class GetRoleByIdHandler : IRequestHandler<GetRoleByIdQuery, ApiResponse<RoleDto>>
{
    private readonly IdentityDbContext _dbContext;

    public GetRoleByIdHandler(IdentityDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<ApiResponse<RoleDto>> Handle(GetRoleByIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var role = await _dbContext.Roles
            .FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken)
            ?? throw new NotFoundAppException($"Role '{query.Id}' was not found.");

        return new ApiResponse<RoleDto>
        {
            Status = StatusCodes.Status200OK,
            Data = role.ToRoleDto()
        };
    }
}
