namespace Commerce.Modules.Sale.Application.Policies;

public sealed class SocialChannelPolicy : ISaleChannelPolicy
{
    public bool CanOverridePrice() => false;
    public bool CanApplyManualDiscount() => false;
    public bool SupportsFastCheckout() => false;
    public bool RequiresStrictValidation() => true;
}
