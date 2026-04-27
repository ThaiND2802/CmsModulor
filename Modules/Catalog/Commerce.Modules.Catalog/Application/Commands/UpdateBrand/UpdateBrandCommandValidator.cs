using FluentValidation;

namespace Commerce.Modules.Catalog.Application.Commands.UpdateBrand;

public sealed class UpdateBrandCommandValidator : AbstractValidator<UpdateBrandCommand>
{
    public UpdateBrandCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(200).Matches("^[a-z0-9-]+$");
        RuleFor(x => x.LogoUrl)
            .MaximumLength(2000)
            .Must(value => string.IsNullOrWhiteSpace(value) || BeValidHttpUrl(value))
            .WithMessage("LogoUrl must be a valid absolute HTTP or HTTPS URL.");
        RuleFor(x => x.Website)
            .MaximumLength(2000)
            .Must(value => string.IsNullOrWhiteSpace(value) || BeValidHttpUrl(value))
            .WithMessage("Website must be a valid absolute HTTP or HTTPS URL.");
    }

    private static bool BeValidHttpUrl(string? value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
