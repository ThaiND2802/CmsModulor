using Commerce.Modules.Sale.Domain;
using FluentAssertions;
using Xunit;

namespace Commerce.Modules.Sale.Tests.SaleLifecycle;
public sealed class SaleLifecycleTransitionsTests
{
    [Theory]
    [InlineData(SaleStatus.Draft, SaleStatus.Draft)]
    [InlineData(SaleStatus.Draft, SaleStatus.Priced)]
    [InlineData(SaleStatus.Draft, SaleStatus.Cancelled)]
    [InlineData(SaleStatus.Draft, SaleStatus.Expired)]
    [InlineData(SaleStatus.Priced, SaleStatus.Priced)]
    [InlineData(SaleStatus.Priced, SaleStatus.Draft)]
    [InlineData(SaleStatus.Priced, SaleStatus.Submitted)]
    [InlineData(SaleStatus.Priced, SaleStatus.Cancelled)]
    [InlineData(SaleStatus.Priced, SaleStatus.Expired)]
    [InlineData(SaleStatus.Submitted, SaleStatus.Submitted)]
    [InlineData(SaleStatus.Cancelled, SaleStatus.Cancelled)]
    [InlineData(SaleStatus.Expired, SaleStatus.Expired)]
    [InlineData(SaleStatus.Expired, SaleStatus.Priced)]
    public void CanTransition_ReturnsTrue_ForAllowedAndIdempotentCases(SaleStatus from, SaleStatus to)
    {
        SaleLifecycleTransitions.CanTransition(from, to).Should().BeTrue();
    }

    [Theory]
    [InlineData(SaleStatus.Draft, SaleStatus.Submitted)]
    [InlineData(SaleStatus.Submitted, SaleStatus.Draft)]
    [InlineData(SaleStatus.Submitted, SaleStatus.Priced)]
    [InlineData(SaleStatus.Submitted, SaleStatus.Cancelled)]
    [InlineData(SaleStatus.Submitted, SaleStatus.Expired)]
    [InlineData(SaleStatus.Cancelled, SaleStatus.Draft)]
    [InlineData(SaleStatus.Cancelled, SaleStatus.Priced)]
    [InlineData(SaleStatus.Cancelled, SaleStatus.Submitted)]
    [InlineData(SaleStatus.Cancelled, SaleStatus.Expired)]
    [InlineData(SaleStatus.Expired, SaleStatus.Draft)]
    [InlineData(SaleStatus.Expired, SaleStatus.Submitted)]
    [InlineData(SaleStatus.Expired, SaleStatus.Cancelled)]
    public void CanTransition_ReturnsFalse_ForRejectedCases(SaleStatus from, SaleStatus to)
    {
        SaleLifecycleTransitions.CanTransition(from, to).Should().BeFalse();
    }
}
