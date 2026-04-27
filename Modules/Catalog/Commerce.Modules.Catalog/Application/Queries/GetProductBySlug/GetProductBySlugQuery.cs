using Commerce.Modules.Catalog.Application.DTOs.Responses;
using MediatR;

namespace Commerce.Modules.Catalog.Application.Queries.GetProductBySlug;

public sealed record GetProductBySlugQuery(string Slug) : IRequest<ProductDto>;
