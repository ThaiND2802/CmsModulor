using Commerce.Modules.Payment.Application.DTOs.Responses;
using Commerce.Modules.Payment.Domain;

namespace Commerce.Modules.Payment.Application.Mappings;

public static class PaymentMethodMappings
{
    public static PaymentMethodDto ToPaymentMethodDto(this PaymentMethod entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new PaymentMethodDto(
            entity.Id,
            entity.Code,
            entity.Name,
            entity.IsActive,
            entity.CreatedAtUtc,
            entity.CreatedBy,
            entity.UpdatedAtUtc,
            entity.UpdatedBy);
    }

    public static DeletedPaymentMethodDto ToDeletedPaymentMethodDto(this PaymentMethod entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new DeletedPaymentMethodDto(
            entity.Id,
            entity.Code,
            entity.Name,
            entity.DeletedAtUtc,
            entity.DeletedBy);
    }
}
