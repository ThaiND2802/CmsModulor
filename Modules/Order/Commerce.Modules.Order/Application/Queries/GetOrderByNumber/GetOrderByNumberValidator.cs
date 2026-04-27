using FluentValidation;

namespace Commerce.Modules.Order.Application.Queries.GetOrderByNumber;

public sealed class GetOrderByNumberValidator : AbstractValidator<GetOrderByNumberQuery>
{
    public GetOrderByNumberValidator()
    {
        RuleFor(x => x.OrderNumber)
            .NotEmpty()
            .MaximumLength(50);
    }
}
