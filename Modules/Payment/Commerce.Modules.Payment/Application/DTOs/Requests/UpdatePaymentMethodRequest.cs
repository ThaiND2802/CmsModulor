namespace Commerce.Modules.Payment.Application.DTOs.Requests;

public sealed record UpdatePaymentMethodRequest(string Name, bool IsActive);
