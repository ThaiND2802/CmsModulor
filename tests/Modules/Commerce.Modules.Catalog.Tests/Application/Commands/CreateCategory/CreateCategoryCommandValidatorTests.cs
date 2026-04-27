using Commerce.Modules.Catalog.Application.Commands.CreateCategory;
using FluentAssertions;

namespace Commerce.Modules.Catalog.Tests.Application.Commands.CreateCategory;

public sealed class CreateCategoryCommandValidatorTests
{
    private readonly CreateCategoryCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldNotHaveErrors_WhenCommandIsValid()
    {
        var command = new CreateCategoryCommand("Shoes", "shoes", "desc", null, 0);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldHaveError_ForName_WhenEmpty()
    {
        var command = new CreateCategoryCommand(string.Empty, "shoes", null, null, 0);

        var result = _validator.Validate(command);

        result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(CreateCategoryCommand.Name));
    }

    [Fact]
    public void Validate_ShouldHaveError_ForSlug_WhenInvalid()
    {
        var command = new CreateCategoryCommand("Shoes", "Hello World", null, null, 0);

        var result = _validator.Validate(command);

        result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(CreateCategoryCommand.Slug));
    }

    [Fact]
    public void Validate_ShouldHaveError_ForDisplayOrder_WhenNegative()
    {
        var command = new CreateCategoryCommand("Shoes", "shoes", null, null, -1);

        var result = _validator.Validate(command);

        result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(CreateCategoryCommand.DisplayOrder));
    }
}
