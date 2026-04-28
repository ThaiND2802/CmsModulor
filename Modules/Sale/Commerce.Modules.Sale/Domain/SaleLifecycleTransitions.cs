namespace Commerce.Modules.Sale.Domain;

public static class SaleLifecycleTransitions
{
    public static bool CanTransition(SaleStatus currentStatus, SaleStatus targetStatus)
    {
        return currentStatus == targetStatus || targetStatus switch
        {
            SaleStatus.Draft => currentStatus == SaleStatus.Priced,
            SaleStatus.Priced => currentStatus == SaleStatus.Draft || currentStatus == SaleStatus.Expired,
            SaleStatus.Submitted => currentStatus == SaleStatus.Priced,
            SaleStatus.Cancelled => currentStatus is SaleStatus.Draft or SaleStatus.Priced,
            SaleStatus.Expired => currentStatus is SaleStatus.Draft or SaleStatus.Priced,
            _ => false
        };
    }

    public static void EnsureCanTransition(SaleStatus currentStatus, SaleStatus targetStatus, string action)
    {
        if (!CanTransition(currentStatus, targetStatus))
        {
            throw new InvalidOperationException($"Cannot {action} when sale is in {currentStatus} status.");
        }
    }
}
