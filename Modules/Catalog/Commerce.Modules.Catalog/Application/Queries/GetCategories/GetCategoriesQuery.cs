using Commerce.Modules.Catalog.Application.DTOs.Responses;
using MediatR;

namespace Commerce.Modules.Catalog.Application.Queries.GetCategories;

public sealed record GetCategoriesQuery(bool TreeView = true) : IRequest<IReadOnlyList<CategoryDto>>;
