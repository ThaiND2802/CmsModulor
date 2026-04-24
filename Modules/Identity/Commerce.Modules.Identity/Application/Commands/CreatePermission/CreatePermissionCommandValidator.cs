using FluentValidation;

namespace Commerce.Modules.Identity.Application.Commands.CreatePermission;

public sealed class CreatePermissionCommandValidator : AbstractValidator<CreatePermissionCommand>
{
    public CreatePermissionCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(150);

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
