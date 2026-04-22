using Commerce.Modules.Payment.Application.DTOs.Responses;
using Commerce.Modules.Payment.Domain;

namespace Commerce.Modules.Payment.Application.Mappings;

public static class PaymentMethodMappings
{
    public static PaymentMethodDto ToPaymentMethodDto(this PaymentMethod entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new PaymentMethodDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            IsActive = entity.IsActive,
            CreatedAtUtc = entity.CreatedAtUtc,
            CreatedBy = entity.CreatedBy,
            UpdatedAtUtc = entity.UpdatedAtUtc,
            UpdatedBy = entity.UpdatedBy
        };
    }

    public static DeletedPaymentMethodDto ToDeletedPaymentMethodDto(this PaymentMethod entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new DeletedPaymentMethodDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            DeletedAtUtc = entity.DeletedAtUtc,
            DeletedBy = entity.DeletedBy
        };
    }
}
