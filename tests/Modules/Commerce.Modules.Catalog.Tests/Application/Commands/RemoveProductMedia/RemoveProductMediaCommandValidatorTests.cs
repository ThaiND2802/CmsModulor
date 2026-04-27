using Commerce.Modules.Catalog.Application.Commands.RemoveProductMedia;
using FluentAssertions;

namespace Commerce.Modules.Catalog.Tests.Application.Commands.RemoveProductMedia;

public sealed class RemoveProductMediaCommandValidatorTests
{
    private readonly RemoveProductMediaCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldNotHaveErrors_WhenIdsAreValid()
    {
        var result = _validator.Validate(new RemoveProductMediaCommand(Guid.NewGuid(), Guid.NewGuid()));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenProductIdIsEmpty()
    {
        var result = _validator.Validate(new RemoveProductMediaCommand(Guid.Empty, Guid.NewGuid()));

        result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(RemoveProductMediaCommand.ProductId));
    }
}
