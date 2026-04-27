namespace Commerce.Modules.Sale.Domain;

public enum SaleStatus
{
    Draft = 0,
    Priced = 1,
    Submitted = 2,
    Cancelled = 3,
    Reserved = 4,
    AwaitingPayment = 5,
    Paid = 6,
    Expired = 7
}
