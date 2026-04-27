using AutoMapper;
using Commerce.Modules.Sale.Application.DTOs.Responses;
using Commerce.Modules.Sale.Domain;
using SaleEntity = Commerce.Modules.Sale.Domain.Sale;

namespace Commerce.Modules.Sale.Application.Mappings;

public sealed class SaleMappingProfile : Profile
{
    public SaleMappingProfile()
    {
        CreateMap<SaleAddress, SaleAddressDto>();
        CreateMap<SaleItem, SaleItemDto>()
            .ForCtorParam(nameof(SaleItemDto.Override), o => o.MapFrom(s =>
                s.OverridePrice.HasValue && !string.IsNullOrWhiteSpace(s.OverrideReason) && s.OverriddenAtUtc.HasValue
                    ? new SaleItemOverrideDto(
                        s.OverridePrice.Value,
                        s.OverrideReason!,
                        s.OverriddenBy,
                        s.OverriddenAtUtc.Value)
                    : null));
        CreateMap<SaleStatusHistory, SaleStatusHistoryDto>()
            .ForCtorParam(nameof(SaleStatusHistoryDto.FromStatus), o => o.MapFrom(s => s.FromStatus.HasValue ? s.FromStatus.Value.ToString() : null))
            .ForCtorParam(nameof(SaleStatusHistoryDto.ToStatus), o => o.MapFrom(s => s.ToStatus.ToString()));
        CreateMap<SaleEntity, SaleListItemDto>()
            .ForCtorParam(nameof(SaleListItemDto.Status), o => o.MapFrom(s => s.Status.ToString()))
            .ForCtorParam(nameof(SaleListItemDto.ItemCount), o => o.MapFrom(s => s.Items.Count));
        CreateMap<SaleEntity, SaleDto>()
            .ForCtorParam(nameof(SaleDto.Status), o => o.MapFrom(s => s.Status.ToString()))
            .ForCtorParam(nameof(SaleDto.ExpiresAtUtc), o => o.MapFrom(s => s.ExpiresAtUtc))
            .ForCtorParam(nameof(SaleDto.Coupon), o => o.MapFrom(s =>
                string.IsNullOrWhiteSpace(s.CouponCode) || string.IsNullOrWhiteSpace(s.CouponType)
                    ? null
                    : new SaleCouponDto(
                        s.CouponCode!,
                        s.CouponType!,
                        s.CouponValue ?? 0m)))
            .ForCtorParam(nameof(SaleDto.Totals), o => o.MapFrom(s => new SaleTotalsDto(
                s.SubtotalAmount,
                s.DiscountAmount,
                s.ShippingAmount,
                s.TaxAmount,
                s.TotalAmount,
                s.Currency)));
    }
}
