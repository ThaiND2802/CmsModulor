using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Application.Mappings;
using Commerce.Modules.Identity.Infrastructure;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Identity.Application.Queries.GetPermissionById;

public sealed class GetPermissionByIdHandler : IRequestHandler<GetPermissionByIdQuery, ApiResponse<PermissionDto>>
{
    private readonly IdentityDbContext _dbContext;

    public GetPermissionByIdHandler(IdentityDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<ApiResponse<PermissionDto>> Handle(GetPermissionByIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var permission = await _dbContext.Permissions
            .FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken)
            ?? throw new NotFoundAppException($"Permission '{query.Id}' was not found.");

        return new ApiResponse<PermissionDto>
        {
            Status = StatusCodes.Status200OK,
            Data = permission.ToPermissionDto()
        };
    }
}
