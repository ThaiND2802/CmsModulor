using Commerce.Modules.Catalog.Application.Commands.UpdateBrand;
using FluentAssertions;

namespace Commerce.Modules.Catalog.Tests.Application.Commands.UpdateBrand;

public sealed class UpdateBrandCommandValidatorTests
{
    private readonly UpdateBrandCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldNotHaveErrors_WhenCommandIsValid()
    {
        var result = _validator.Validate(new UpdateBrandCommand(Guid.NewGuid(), "Nike", "nike", "https://example.com/logo.png", "https://example.com", true));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldHaveError_ForId_WhenEmpty()
    {
        var result = _validator.Validate(new UpdateBrandCommand(Guid.Empty, "Nike", "nike", null, null, true));

        result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(UpdateBrandCommand.Id));
    }

    [Fact]
    public void Validate_ShouldHaveError_ForWebsite_WhenSchemeIsInvalid()
    {
        var result = _validator.Validate(new UpdateBrandCommand(Guid.NewGuid(), "Nike", "nike", null, "ftp://example.com", true));

        result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(UpdateBrandCommand.Website));
    }
}
