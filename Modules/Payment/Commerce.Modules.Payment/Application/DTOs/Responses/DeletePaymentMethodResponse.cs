namespace Commerce.Modules.Payment.Application.DTOs.Responses;

public sealed record DeletePaymentMethodResponse
{
    public Guid Id { get; init; }

    public bool Deleted { get; init; }
}
