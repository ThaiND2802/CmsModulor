using Commerce.Modules.Catalog.Application.DTOs.Responses;
using MediatR;

namespace Commerce.Modules.Catalog.Application.Queries.GetProductById;

public sealed record GetProductByIdQuery(Guid Id) : IRequest<ProductDto>;
