using FluentValidation;

namespace Commerce.Modules.Sale.Application.Commands.RemoveCoupon;

public sealed class RemoveCouponCommandValidator : AbstractValidator<RemoveCouponCommand>
{
    public RemoveCouponCommandValidator()
    {
        RuleFor(x => x.SaleId)
            .NotEmpty();
    }
}
