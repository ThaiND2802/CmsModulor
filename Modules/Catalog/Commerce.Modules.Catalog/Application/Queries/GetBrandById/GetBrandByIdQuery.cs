using Commerce.Modules.Catalog.Application.DTOs.Responses;
using MediatR;

namespace Commerce.Modules.Catalog.Application.Queries.GetBrandById;

public sealed record GetBrandByIdQuery(Guid Id) : IRequest<BrandDto>;
