using FluentValidation;

namespace Commerce.Modules.Identity.Application.Commands.SyncUserPermissions;

public sealed class SyncUserPermissionsCommandValidator : AbstractValidator<SyncUserPermissionsCommand>
{
    public SyncUserPermissionsCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.Permissions)
            .NotNull();

        RuleFor(x => x.Permissions)
            .Must(x => x.DistinctBy(y => y.PermissionId).Count() == x.Count)
            .WithMessage("Duplicate permission ids are not allowed.");

        RuleForEach(x => x.Permissions)
            .SetValidator(new SyncUserPermissionItemValidator());
    }
}

public sealed class SyncUserPermissionItemValidator : AbstractValidator<SyncUserPermissionItem>
{
    public SyncUserPermissionItemValidator()
    {
        RuleFor(x => x.PermissionId)
            .NotEmpty();

        RuleFor(x => x.Effect)
            .NotEmpty()
            .Must(x => x.Trim().ToUpperInvariant() is "ALLOW" or "DENY")
            .WithMessage("'Effect' must be either 'Allow' or 'Deny'.");

        RuleFor(x => x.ExpiresAtUtc)
            .Must(x => !x.HasValue || x.Value.Kind == DateTimeKind.Utc)
            .WithMessage("Permission expiry must be provided in UTC.");

        RuleFor(x => x.ExpiresAtUtc)
            .Must(x => !x.HasValue || x.Value > DateTime.UtcNow)
            .WithMessage("Permission expiry must be greater than the current time.");
    }
}
