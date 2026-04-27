using FluentValidation;

namespace Commerce.Modules.Sale.Application.Commands.UpdateSaleAddresses;

public sealed class UpdateSaleAddressesCommandValidator : AbstractValidator<UpdateSaleAddressesCommand>
{
    public UpdateSaleAddressesCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        When(x => x.ShippingAddress is not null, () =>
        {
            RuleFor(x => x.ShippingAddress!.FullName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.ShippingAddress!.PhoneNumber).NotEmpty().MaximumLength(50);
            RuleFor(x => x.ShippingAddress!.AddressLine1).NotEmpty().MaximumLength(500);
            RuleFor(x => x.ShippingAddress!.AddressLine2).MaximumLength(500);
            RuleFor(x => x.ShippingAddress!.City).NotEmpty().MaximumLength(200);
            RuleFor(x => x.ShippingAddress!.State).MaximumLength(200);
            RuleFor(x => x.ShippingAddress!.PostalCode).MaximumLength(20);
            RuleFor(x => x.ShippingAddress!.Country).NotEmpty().MaximumLength(100);
        });

        When(x => x.BillingAddress is not null, () =>
        {
            RuleFor(x => x.BillingAddress!.FullName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.BillingAddress!.PhoneNumber).NotEmpty().MaximumLength(50);
            RuleFor(x => x.BillingAddress!.AddressLine1).NotEmpty().MaximumLength(500);
            RuleFor(x => x.BillingAddress!.AddressLine2).MaximumLength(500);
            RuleFor(x => x.BillingAddress!.City).NotEmpty().MaximumLength(200);
            RuleFor(x => x.BillingAddress!.State).MaximumLength(200);
            RuleFor(x => x.BillingAddress!.PostalCode).MaximumLength(20);
            RuleFor(x => x.BillingAddress!.Country).NotEmpty().MaximumLength(100);
        });
    }
}
