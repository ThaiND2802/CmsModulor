using FluentValidation;

namespace Commerce.Modules.Identity.Application.Commands.UpdatePermission;

public sealed class UpdatePermissionCommandValidator : AbstractValidator<UpdatePermissionCommand>
{
    public UpdatePermissionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.Module)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Feature)
            .MaximumLength(100);

        RuleFor(x => x.GroupName)
            .NotEmpty()
            .MaximumLength(100);
    }
}
