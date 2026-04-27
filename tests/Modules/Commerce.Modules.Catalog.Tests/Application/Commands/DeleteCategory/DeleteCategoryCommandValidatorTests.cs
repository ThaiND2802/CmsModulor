using Commerce.Modules.Catalog.Application.Commands.DeleteCategory;
using FluentAssertions;

namespace Commerce.Modules.Catalog.Tests.Application.Commands.DeleteCategory;

public sealed class DeleteCategoryCommandValidatorTests
{
    private readonly DeleteCategoryCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldNotHaveErrors_WhenIdIsValid()
    {
        var result = _validator.Validate(new DeleteCategoryCommand(Guid.NewGuid()));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenIdIsEmpty()
    {
        var result = _validator.Validate(new DeleteCategoryCommand(Guid.Empty));

        result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(DeleteCategoryCommand.Id));
    }
}
