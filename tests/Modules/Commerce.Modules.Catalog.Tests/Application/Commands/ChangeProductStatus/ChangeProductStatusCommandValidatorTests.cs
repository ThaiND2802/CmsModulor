using Commerce.Modules.Catalog.Application.Commands.ChangeProductStatus;
using Commerce.Modules.Catalog.Domain;
using FluentAssertions;

namespace Commerce.Modules.Catalog.Tests.Application.Commands.ChangeProductStatus;

public sealed class ChangeProductStatusCommandValidatorTests
{
    private readonly ChangeProductStatusCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldNotHaveErrors_WhenCommandIsValid()
    {
        var result = _validator.Validate(new ChangeProductStatusCommand(Guid.NewGuid(), ProductStatus.Active));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldHaveError_WhenIdIsEmpty()
    {
        var result = _validator.Validate(new ChangeProductStatusCommand(Guid.Empty, ProductStatus.Active));

        result.Errors.Should().ContainSingle(error => error.PropertyName == nameof(ChangeProductStatusCommand.Id));
    }
}
