namespace Commerce.Modules.Sale.Application.Policies;

public sealed class PosChannelPolicy : ISaleChannelPolicy
{
    public bool CanOverridePrice() => false;
    public bool CanApplyManualDiscount() => true;
    public bool SupportsFastCheckout() => true;
    public bool RequiresStrictValidation() => false;
}
