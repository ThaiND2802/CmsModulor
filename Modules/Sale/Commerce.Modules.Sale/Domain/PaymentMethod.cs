namespace Commerce.Modules.Sale.Domain;

public enum PaymentMethod
{
    None = 0,
    Cash = 1,
    CreditCard = 2,
    DebitCard = 3,
    BankTransfer = 4,
    EWallet = 5,
    COD = 6
}
