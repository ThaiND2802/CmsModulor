using FluentValidation;

namespace Commerce.Modules.Sale.Application.Commands.UpdateSaleCustomer;

public sealed class UpdateSaleCustomerCommandValidator : AbstractValidator<UpdateSaleCustomerCommand>
{
    public UpdateSaleCustomerCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.CustomerEmail).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.CustomerPhone).MaximumLength(50);
    }
}
