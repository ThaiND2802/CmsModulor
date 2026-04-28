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
            SaleChannel.Admin or SaleChannel.Social => throw new InvalidOperationException(
                $"Channel '{channel}' is disabled in the MVP runtime."),
            _ => throw new ArgumentOutOfRangeException(nameof(channel), channel, "Unknown channel")
        };
    }
}
