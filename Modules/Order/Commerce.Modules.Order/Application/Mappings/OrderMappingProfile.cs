using AutoMapper;
using Commerce.Modules.Order.Application.DTOs.Responses;
using Commerce.Modules.Order.Domain;
using OrderEntity = Commerce.Modules.Order.Domain.Order;

namespace Commerce.Modules.Order.Application.Mappings;

public sealed class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        CreateMap<OrderAddress, OrderAddressDto>();
        CreateMap<OrderItem, OrderItemDto>();
        CreateMap<OrderStatusHistory, OrderStatusHistoryDto>()
            .ForCtorParam(nameof(OrderStatusHistoryDto.FromStatus), o => o.MapFrom(s => s.FromStatus.HasValue ? s.FromStatus.Value.ToString() : null))
            .ForCtorParam(nameof(OrderStatusHistoryDto.ToStatus), o => o.MapFrom(s => s.ToStatus.ToString()));
        CreateMap<OrderEntity, OrderListItemDto>()
            .ForCtorParam(nameof(OrderListItemDto.Status), o => o.MapFrom(s => s.Status.ToString()))
            .ForCtorParam(nameof(OrderListItemDto.ItemCount), o => o.MapFrom(s => s.Items.Count));
        CreateMap<OrderEntity, OrderDto>()
            .ForCtorParam(nameof(OrderDto.Status), o => o.MapFrom(s => s.Status.ToString()));
    }
}
