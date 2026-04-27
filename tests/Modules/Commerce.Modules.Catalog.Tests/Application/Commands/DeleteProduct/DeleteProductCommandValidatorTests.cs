using Commerce.Modules.Catalog.Application.Commands.DeleteProduct;
using FluentAssertions;

namespace Commerce.Modules.Catalog.Tests.Application.Commands.DeleteProduct;

public sealed class DeleteProductCommandValidatorTests
{
    private readonly DeleteProductCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldNotHaveErrors_WhenIdIsValid()
    {
        var result = _validator.Validate(new DeleteProductCommand(Guid.NewGuid()));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenIdIsEmpty()
    {
        var result = _validator.Validate(new DeleteProductCommand(Guid.Empty));

        result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(DeleteProductCommand.Id));
    }
}
