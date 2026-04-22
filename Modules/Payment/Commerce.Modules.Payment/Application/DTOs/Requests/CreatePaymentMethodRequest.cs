namespace Commerce.Modules.Payment.Application.DTOs.Requests;

public sealed record CreatePaymentMethodRequest(string Code, string Name, bool IsActive);
