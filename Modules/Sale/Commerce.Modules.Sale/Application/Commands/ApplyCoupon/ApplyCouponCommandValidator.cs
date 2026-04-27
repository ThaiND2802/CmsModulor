using FluentValidation;

namespace Commerce.Modules.Sale.Application.Commands.ApplyCoupon;

public sealed class ApplyCouponCommandValidator : AbstractValidator<ApplyCouponCommand>
{
    public ApplyCouponCommandValidator()
    {
        RuleFor(x => x.SaleId)
            .NotEmpty();

        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(100);
    }
}
