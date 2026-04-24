using FluentValidation;

namespace Commerce.Modules.Identity.Application.Commands.SyncUserRoles;

public sealed class SyncUserRolesCommandValidator : AbstractValidator<SyncUserRolesCommand>
{
    public SyncUserRolesCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.RoleIds)
            .NotNull();

        RuleFor(x => x.RoleIds)
            .Must(x => x.Distinct().Count() == x.Count)
            .WithMessage("Duplicate role ids are not allowed.");
    }
}
