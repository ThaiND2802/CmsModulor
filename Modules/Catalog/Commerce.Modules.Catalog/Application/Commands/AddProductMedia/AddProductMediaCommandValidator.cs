using FluentValidation;

namespace Commerce.Modules.Catalog.Application.Commands.AddProductMedia;

public sealed class AddProductMediaCommandValidator : AbstractValidator<AddProductMediaCommand>
{
    public AddProductMediaCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Url)
            .NotEmpty()
            .MaximumLength(2000)
            .Must(BeValidHttpUrl)
            .WithMessage("Url must be a valid absolute HTTP or HTTPS URL.");
        RuleFor(x => x.AltText).MaximumLength(500);
        RuleFor(x => x.MediaType).IsInEnum();
    }

    private static bool BeValidHttpUrl(string? value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
