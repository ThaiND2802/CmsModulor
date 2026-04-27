using AutoMapper;
using Commerce.Modules.Catalog.Application.DTOs.Responses;
using Commerce.Modules.Catalog.Domain;

namespace Commerce.Modules.Catalog.Application.Mappings;

public sealed class CatalogMappingProfile : Profile
{
    public CatalogMappingProfile()
    {
        CreateMap<Category, CategoryDto>()
            .ForCtorParam(nameof(CategoryDto.Children), options => options.MapFrom(source => source.Children));

        CreateMap<Brand, BrandDto>();

        CreateMap<Product, ProductListItemDto>()
            .ForCtorParam(nameof(ProductListItemDto.CategoryName), options => options.MapFrom(source => source.Category.Name))
            .ForCtorParam(nameof(ProductListItemDto.BrandName), options => options.MapFrom(source => source.Brand != null ? source.Brand.Name : null))
            .ForCtorParam(nameof(ProductListItemDto.CreatedAtUtc), options => options.MapFrom(source => new DateTimeOffset(DateTime.SpecifyKind(source.CreatedAtUtc, DateTimeKind.Utc))));

        CreateMap<Product, ProductDto>()
            .ForCtorParam(nameof(ProductDto.CategoryName), options => options.MapFrom(source => source.Category.Name))
            .ForCtorParam(nameof(ProductDto.BrandName), options => options.MapFrom(source => source.Brand != null ? source.Brand.Name : null))
            .ForCtorParam(nameof(ProductDto.Media), options => options.MapFrom(source => source.Media))
            .ForCtorParam(nameof(ProductDto.Variants), options => options.MapFrom(source => source.Variants))
            .ForCtorParam(nameof(ProductDto.CreatedAtUtc), options => options.MapFrom(source => new DateTimeOffset(DateTime.SpecifyKind(source.CreatedAtUtc, DateTimeKind.Utc))))
            .ForCtorParam(nameof(ProductDto.UpdatedAtUtc), options => options.MapFrom(source => source.UpdatedAtUtc.HasValue
                ? new DateTimeOffset(DateTime.SpecifyKind(source.UpdatedAtUtc.Value, DateTimeKind.Utc))
                : (DateTimeOffset?)null));

        CreateMap<ProductMedia, ProductMediaDto>();

        CreateMap<ProductVariant, ProductVariantDto>()
            .ForCtorParam(nameof(ProductVariantDto.Options), options => options.MapFrom(source => source.Options));

        CreateMap<ProductVariantOption, ProductVariantOptionDto>()
            .ForCtorParam(nameof(ProductVariantOptionDto.AttributeName), options => options.MapFrom(source => source.Attribute.DisplayName));
    }
}
