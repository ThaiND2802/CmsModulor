namespace Commerce.Modules.Sale.Application.Policies;

public interface ISaleChannelPolicy
{
    bool CanOverridePrice();
    bool CanApplyManualDiscount();
    bool SupportsFastCheckout();
    bool RequiresStrictValidation();
}
