using Commerce.Modules.Catalog.Application.Commands.DeleteBrand;
using FluentAssertions;

namespace Commerce.Modules.Catalog.Tests.Application.Commands.DeleteBrand;

public sealed class DeleteBrandCommandValidatorTests
{
    private readonly DeleteBrandCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldNotHaveErrors_WhenIdIsValid()
    {
        var result = _validator.Validate(new DeleteBrandCommand(Guid.NewGuid()));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenIdIsEmpty()
    {
        var result = _validator.Validate(new DeleteBrandCommand(Guid.Empty));

        result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(DeleteBrandCommand.Id));
    }
}
