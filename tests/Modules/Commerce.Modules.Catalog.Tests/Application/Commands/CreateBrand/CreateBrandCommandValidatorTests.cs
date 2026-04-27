using Commerce.Modules.Catalog.Application.Commands.CreateBrand;
using FluentAssertions;

namespace Commerce.Modules.Catalog.Tests.Application.Commands.CreateBrand;

public sealed class CreateBrandCommandValidatorTests
{
    private readonly CreateBrandCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldNotHaveErrors_WhenCommandIsValid()
    {
        var result = _validator.Validate(new CreateBrandCommand("Nike", "nike", "https://example.com/logo.png", "https://example.com"));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldHaveError_ForSlug_WhenInvalid()
    {
        var result = _validator.Validate(new CreateBrandCommand("Nike", "Nike Brand", null, null));

        result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(CreateBrandCommand.Slug));
    }

    [Fact]
    public void Validate_ShouldHaveError_ForLogoUrl_WhenSchemeIsInvalid()
    {
        var result = _validator.Validate(new CreateBrandCommand("Nike", "nike", "ftp://example.com/logo.png", null));

        result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(CreateBrandCommand.LogoUrl));
    }
}
