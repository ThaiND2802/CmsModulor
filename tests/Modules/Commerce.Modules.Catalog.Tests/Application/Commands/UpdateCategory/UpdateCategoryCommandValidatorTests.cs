using Commerce.Modules.Catalog.Application.Commands.UpdateCategory;
using FluentAssertions;

namespace Commerce.Modules.Catalog.Tests.Application.Commands.UpdateCategory;

public sealed class UpdateCategoryCommandValidatorTests
{
    private readonly UpdateCategoryCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldNotHaveErrors_WhenCommandIsValid()
    {
        var result = _validator.Validate(new UpdateCategoryCommand(Guid.NewGuid(), "Shoes", "shoes", "Desc", null, 0, true));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldHaveError_ForId_WhenEmpty()
    {
        var result = _validator.Validate(new UpdateCategoryCommand(Guid.Empty, "Shoes", "shoes", null, null, 0, true));

        result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(UpdateCategoryCommand.Id));
    }

    [Fact]
    public void Validate_ShouldHaveError_ForDisplayOrder_WhenNegative()
    {
        var result = _validator.Validate(new UpdateCategoryCommand(Guid.NewGuid(), "Shoes", "shoes", null, null, -1, true));

        result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(UpdateCategoryCommand.DisplayOrder));
    }
}
