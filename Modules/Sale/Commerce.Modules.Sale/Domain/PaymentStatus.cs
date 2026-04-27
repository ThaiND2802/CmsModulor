namespace Commerce.Modules.Sale.Domain;

public enum PaymentStatus
{
    None = 0,
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4,
    Refunded = 5
}
