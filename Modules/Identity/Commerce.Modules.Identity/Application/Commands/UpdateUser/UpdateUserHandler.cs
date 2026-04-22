using Commerce.Modules.Identity.Application.DTOs.Responses;
using Commerce.Modules.Identity.Application.Mappings;
using Commerce.Modules.Identity.Domain;
using Commerce.Modules.Identity.Infrastructure;
using Commerce.Modules.Identity.Infrastructure.Authentication;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Identity.Application.Commands.UpdateUser;

public sealed class UpdateUserHandler : IRequestHandler<UpdateUserCommand, ApiResponse<UserDto>>
{
    private readonly IdentityDbContext _dbContext;
    private readonly IPasswordHasherService _passwordHasherService;

    public UpdateUserHandler(
        IdentityDbContext dbContext,
        IPasswordHasherService passwordHasherService)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _passwordHasherService = passwordHasherService ?? throw new ArgumentNullException(nameof(passwordHasherService));
    }

    public async Task<ApiResponse<UserDto>> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.DisplayName))
        {
            throw new ValidationAppException("Display name is required.");
        }

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken)
            ?? throw new NotFoundAppException($"User '{command.Id}' was not found.");

        if (user.IsSystem)
        {
            throw new ForbiddenAppException($"System user '{command.Id}' cannot be modified.");
        }

        var displayName = command.DisplayName.Trim();
        var email = string.IsNullOrWhiteSpace(command.Email) ? null : command.Email.Trim();
        var normalizedEmail = email?.ToUpperInvariant();
        var phoneNumber = string.IsNullOrWhiteSpace(command.PhoneNumber) ? null : command.PhoneNumber.Trim();

        if (normalizedEmail is not null)
        {
            var emailExists = await _dbContext.Users
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Id != command.Id && x.NormalizedEmail == normalizedEmail, cancellationToken);
            if (emailExists)
            {
                throw new ConflictAppException($"Email '{email}' already exists.");
            }
        }

        user.DisplayName = displayName;
        user.Email = email;
        user.NormalizedEmail = normalizedEmail;
        user.PhoneNumber = phoneNumber;
        user.IsActive = command.IsActive;

        if (!string.IsNullOrWhiteSpace(command.Password))
        {
            user.PasswordHash = _passwordHasherService.Hash(command.Password);
        }

        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResponse<UserDto>
        {
            Status = StatusCodes.Status200OK,
            Data = user.ToUserDto()
        };
    }
}
