using Commerce.Modules.Catalog.Application.DTOs.Responses;
using MediatR;

namespace Commerce.Modules.Catalog.Application.Queries.GetProductVariants;

public sealed record GetProductVariantsQuery(Guid ProductId) : IRequest<IReadOnlyList<ProductVariantDto>>;
