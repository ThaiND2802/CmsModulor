using FluentValidation;

namespace Commerce.Modules.Inventory.Application.Commands.ReleaseStock;

public sealed class ReleaseStockValidator : AbstractValidator<ReleaseStockCommand>
{
    public ReleaseStockValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty();
    }
}
