namespace Commerce.Modules.Sale.Application.Policies;

public sealed class AdminChannelPolicy : ISaleChannelPolicy
{
    public bool CanOverridePrice() => true;
    public bool CanApplyManualDiscount() => true;
    public bool SupportsFastCheckout() => false;
    public bool RequiresStrictValidation() => false;
}
