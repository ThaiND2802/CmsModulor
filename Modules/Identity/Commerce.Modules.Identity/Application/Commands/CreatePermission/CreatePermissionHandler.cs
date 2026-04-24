using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Application.Mappings;
using Commerce.Modules.Identity.Domain;
using Commerce.Modules.Identity.Infrastructure;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Identity.Application.Commands.CreatePermission;

public sealed class CreatePermissionHandler : IRequestHandler<CreatePermissionCommand, ApiResponse<PermissionDto>>
{
    private readonly IdentityDbContext _dbContext;

    public CreatePermissionHandler(IdentityDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<ApiResponse<PermissionDto>> Handle(CreatePermissionCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var code = command.Code.Trim().ToUpperInvariant();
        var name = command.Name.Trim();
        var description = string.IsNullOrWhiteSpace(command.Description) ? null : command.Description.Trim();
        var module = command.Module.Trim();
        var feature = string.IsNullOrWhiteSpace(command.Feature) ? null : command.Feature.Trim();
        var groupName = command.GroupName.Trim();

        var codeExists = await _dbContext.Permissions
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Code == code, cancellationToken);
        if (codeExists)
        {
            throw new ConflictAppException($"Permission code '{code}' already exists.");
        }

        var permission = new Permission
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name,
            Description = description,
            Module = module,
            Feature = feature,
            GroupName = groupName,
            SortOrder = command.SortOrder,
            IsActive = command.IsActive,
            IsSystem = false
        };

        await _dbContext.Permissions.AddAsync(permission, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResponse<PermissionDto>
        {
            Status = StatusCodes.Status201Created,
            Data = permission.ToPermissionDto()
        };
    }
}
