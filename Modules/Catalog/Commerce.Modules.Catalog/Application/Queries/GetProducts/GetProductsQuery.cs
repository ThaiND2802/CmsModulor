using Commerce.Modules.Catalog.Application.DTOs.Responses;
using Commerce.Modules.Catalog.Domain;
using CommerceCore.Application.Abstractions;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Catalog.Application.Queries.GetProducts;

public sealed class GetProductsQuery : PagedListRequest, IRequest<PagedApiResponse<ProductListItemDto>>
{
    public Guid? CategoryId { get; init; }

    public Guid? BrandId { get; init; }

    public ProductStatus? Status { get; init; }

    public bool? IsFeatured { get; init; }
}
