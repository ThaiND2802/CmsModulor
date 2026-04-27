using Commerce.Modules.Catalog.Application.DTOs.Responses;
using CommerceCore.Application.Abstractions;
using CommerceCore.Application.Responses;
using MediatR;

namespace Commerce.Modules.Catalog.Application.Queries.GetBrands;

public sealed class GetBrandsQuery : PagedListRequest, IRequest<PagedApiResponse<BrandDto>>
{
}
