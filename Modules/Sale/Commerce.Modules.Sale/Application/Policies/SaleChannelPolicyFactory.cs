using Commerce.Modules.Sale.Domain;

namespace Commerce.Modules.Sale.Application.Policies;

public sealed class SaleChannelPolicyFactory
{
    public ISaleChannelPolicy GetPolicy(SaleChannel channel)
    {
        return channel switch
        {
            SaleChannel.Web => new WebChannelPolicy(),
            SaleChannel.Pos => new PosChannelPolicy(),
            SaleChannel.Admin => new AdminChannelPolicy(),
            SaleChannel.Social => new SocialChannelPolicy(),
            _ => throw new ArgumentOutOfRangeException(nameof(channel), channel, "Unknown channel")
        };
    }
}
