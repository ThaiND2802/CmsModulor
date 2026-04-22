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

namespace Commerce.Modules.Identity.Application.Commands.CreateUser;

public sealed class CreateUserHandler : IRequestHandler<CreateUserCommand, ApiResponse<UserDto>>
{
    private readonly IdentityDbContext _dbContext;
    private readonly IPasswordHasherService _passwordHasherService;

    public CreateUserHandler(
        IdentityDbContext dbContext,
        IPasswordHasherService passwordHasherService)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _passwordHasherService = passwordHasherService ?? throw new ArgumentNullException(nameof(passwordHasherService));
    }

    public async Task<ApiResponse<UserDto>> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.UserName))
        {
            throw new ValidationAppException("Username is required.");
        }

        if (string.IsNullOrWhiteSpace(command.DisplayName))
        {
            throw new ValidationAppException("Display name is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Password))
        {
            throw new ValidationAppException("Password is required.");
        }

        var normalizedUserName = command.UserName.Trim().ToUpperInvariant();
        var userName = command.UserName.Trim();
        var displayName = command.DisplayName.Trim();
        var email = string.IsNullOrWhiteSpace(command.Email) ? null : command.Email.Trim();
        var normalizedEmail = email?.ToUpperInvariant();
        var phoneNumber = string.IsNullOrWhiteSpace(command.PhoneNumber) ? null : command.PhoneNumber.Trim();

        var userNameExists = await _dbContext.Users
            .IgnoreQueryFilters()
            .AnyAsync(x => x.NormalizedUserName == normalizedUserName, cancellationToken);
        if (userNameExists)
        {
            throw new ConflictAppException($"Username '{userName}' already exists.");
        }

        if (normalizedEmail is not null)
        {
            var emailExists = await _dbContext.Users
                .IgnoreQueryFilters()
                .AnyAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);
            if (emailExists)
            {
                throw new ConflictAppException($"Email '{email}' already exists.");
            }
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = userName,
            NormalizedUserName = normalizedUserName,
            DisplayName = displayName,
            Email = email,
            NormalizedEmail = normalizedEmail,
            PasswordHash = _passwordHasherService.Hash(command.Password),
            PhoneNumber = phoneNumber,
            IsActive = command.IsActive,
            IsSystem = false
        };

        await _dbContext.Users.AddAsync(user, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResponse<UserDto>
        {
            Status = StatusCodes.Status201Created,
            Data = user.ToUserDto()
        };
    }
}
