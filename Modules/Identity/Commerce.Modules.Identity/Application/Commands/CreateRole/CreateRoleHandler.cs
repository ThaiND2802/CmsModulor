using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Application.Mappings;
using Commerce.Modules.Identity.Domain;
using Commerce.Modules.Identity.Infrastructure;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Identity.Application.Commands.CreateRole;

public sealed class CreateRoleHandler : IRequestHandler<CreateRoleCommand, ApiResponse<RoleDto>>
{
    private readonly IdentityDbContext _dbContext;

    public CreateRoleHandler(IdentityDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<ApiResponse<RoleDto>> Handle(CreateRoleCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.Code))
        {
            throw new ValidationAppException("Role code is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            throw new ValidationAppException("Role name is required.");
        }

        var code = command.Code.Trim().ToUpperInvariant();
        var name = command.Name.Trim();
        var description = string.IsNullOrWhiteSpace(command.Description) ? null : command.Description.Trim();

        var codeExists = await _dbContext.Roles
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Code == code, cancellationToken);
        if (codeExists)
        {
            throw new ConflictAppException($"Role code '{code}' already exists.");
        }

        var role = new Role
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name,
            Description = description,
            IsActive = command.IsActive,
            IsSystem = false
        };

        await _dbContext.Roles.AddAsync(role, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResponse<RoleDto>
        {
            Status = StatusCodes.Status201Created,
            Data = role.ToRoleDto()
        };
    }
}
