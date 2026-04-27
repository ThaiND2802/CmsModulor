using AutoMapper;
using Commerce.Modules.Inventory.Application.DTOs.Responses;
using Commerce.Modules.Inventory.Domain;

namespace Commerce.Modules.Inventory.Application.Mappings;

public sealed class InventoryMappingProfile : Profile
{
    public InventoryMappingProfile()
    {
        CreateMap<InventoryItem, InventoryStockDto>()
            .ForCtorParam(nameof(InventoryStockDto.AvailableQuantity), options => options.MapFrom(source => source.AvailableQuantity));

        CreateMap<StockMovement, StockMovementDto>()
            .ForCtorParam(nameof(StockMovementDto.VariantId), options => options.MapFrom(source => source.InventoryItem.VariantId))
            .ForCtorParam(nameof(StockMovementDto.Sku), options => options.MapFrom(source => source.InventoryItem.Sku))
            .ForCtorParam(nameof(StockMovementDto.MovementType), options => options.MapFrom(source => source.MovementType.ToString()));

        CreateMap<StockReservationItem, StockReservationItemDto>()
            .ForCtorParam(nameof(StockReservationItemDto.VariantId), options => options.MapFrom(source => source.InventoryItem.VariantId))
            .ForCtorParam(nameof(StockReservationItemDto.Sku), options => options.MapFrom(source => source.InventoryItem.Sku));

        CreateMap<StockReservation, StockReservationDto>()
            .ForCtorParam(nameof(StockReservationDto.Status), options => options.MapFrom(source => source.Status.ToString()));
    }
}
