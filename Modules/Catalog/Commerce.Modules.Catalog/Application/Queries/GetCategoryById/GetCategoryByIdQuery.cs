using Commerce.Modules.Catalog.Application.DTOs.Responses;
using MediatR;

namespace Commerce.Modules.Catalog.Application.Queries.GetCategoryById;

public sealed record GetCategoryByIdQuery(Guid Id) : IRequest<CategoryDto>;
