using Commerce.Modules.Sale.Application.Commands.AddSaleItem;
using Commerce.Modules.Sale.Application.Commands.UpdateSaleItem;
using FluentAssertions;

namespace Commerce.Modules.Sale.Tests.Application.Commands;

public sealed class SaleItemCommandValidatorsTests
{
    [Fact]
    public void AddSaleItemValidator_ReturnsError_WhenVariantIdOrQuantityIsInvalid()
    {
        var validator = new AddSaleItemCommandValidator();
        var result = validator.Validate(new AddSaleItemCommand
        {
            SaleId = Guid.NewGuid(),
            VariantId = Guid.Empty,
            Quantity = 0
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(AddSaleItemCommand.VariantId));
        result.Errors.Should().Contain(x => x.PropertyName == nameof(AddSaleItemCommand.Quantity));
    }

    [Fact]
    public void UpdateSaleItemValidator_ReturnsError_WhenQuantityIsInvalid()
    {
        var validator = new UpdateSaleItemCommandValidator();
        var result = validator.Validate(new UpdateSaleItemCommand
        {
            SaleId = Guid.NewGuid(),
            ItemId = Guid.NewGuid(),
            VariantId = Guid.NewGuid(),
            Quantity = 0
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(UpdateSaleItemCommand.Quantity));
    }
}
