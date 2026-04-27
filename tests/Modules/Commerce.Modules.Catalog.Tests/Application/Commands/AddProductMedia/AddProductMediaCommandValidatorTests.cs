using Commerce.Modules.Catalog.Application.Commands.AddProductMedia;
using Commerce.Modules.Catalog.Domain;
using FluentAssertions;

namespace Commerce.Modules.Catalog.Tests.Application.Commands.AddProductMedia;

public sealed class AddProductMediaCommandValidatorTests
{
    private readonly AddProductMediaCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldNotHaveErrors_WhenCommandIsValid()
    {
        var result = _validator.Validate(new AddProductMediaCommand(Guid.NewGuid(), "https://example.com/image.jpg", "Alt", MediaType.Image, true));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldHaveError_ForProductId_WhenEmpty()
    {
        var result = _validator.Validate(new AddProductMediaCommand(Guid.Empty, "https://example.com/image.jpg", null, MediaType.Image, false));

        result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(AddProductMediaCommand.ProductId));
    }

    [Fact]
    public void Validate_ShouldHaveError_ForUrl_WhenSchemeIsInvalid()
    {
        var result = _validator.Validate(new AddProductMediaCommand(Guid.NewGuid(), "ftp://example.com/image.jpg", null, MediaType.Image, false));

        result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(AddProductMediaCommand.Url));
    }
}
