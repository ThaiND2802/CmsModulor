using FluentValidation;

namespace Commerce.Modules.Identity.Application.Commands.SyncRolePermissions;

public sealed class SyncRolePermissionsCommandValidator : AbstractValidator<SyncRolePermissionsCommand>
{
    public SyncRolePermissionsCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty();

        RuleFor(x => x.Permissions)
            .NotNull();

        RuleFor(x => x.Permissions)
            .Must(x => x.DistinctBy(y => y.PermissionId).Count() == x.Count)
            .WithMessage("Duplicate permission ids are not allowed.");

        RuleForEach(x => x.Permissions)
            .SetValidator(new SyncRolePermissionItemValidator());
    }
}

public sealed class SyncRolePermissionItemValidator : AbstractValidator<SyncRolePermissionItem>
{
    public SyncRolePermissionItemValidator()
    {
        RuleFor(x => x.PermissionId)
            .NotEmpty();

        RuleFor(x => x.Effect)
            .NotEmpty()
            .Must(x => x.Trim().ToUpperInvariant() is "ALLOW" or "DENY")
            .WithMessage("'Effect' must be either 'Allow' or 'Deny'.");
    }
}
