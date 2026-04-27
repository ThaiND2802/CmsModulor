using AutoMapper;
using Commerce.Modules.Catalog.Application.DTOs.Responses;
using Commerce.Modules.Catalog.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Catalog.Application.Queries.GetCategories;

public sealed class GetCategoriesHandler : IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    private readonly CatalogDbContext _dbContext;
    private readonly IMapper _mapper;

    public GetCategoriesHandler(CatalogDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<IReadOnlyList<CategoryDto>> Handle(GetCategoriesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var categories = await _dbContext.Categories
            .AsNoTracking()
            .OrderBy(static x => x.DisplayOrder)
            .ThenBy(static x => x.Name)
            .ToListAsync(cancellationToken);

        if (!query.TreeView)
        {
            return categories
                .Select(category => _mapper.Map<CategoryDto>(category) with { Children = Array.Empty<CategoryDto>() })
                .ToList();
        }

        var categoriesByParent = categories
            .Where(static x => x.ParentId.HasValue)
            .GroupBy(static x => x.ParentId!.Value)
            .ToDictionary(static x => x.Key, static x => x.ToList());

        CategoryDto MapCategory(Commerce.Modules.Catalog.Domain.Category category, HashSet<Guid> path)
        {
            if (!path.Add(category.Id))
            {
                return _mapper.Map<CategoryDto>(category) with { Children = Array.Empty<CategoryDto>() };
            }

            var children = categoriesByParent.TryGetValue(category.Id, out var childCategories)
                ? childCategories.Select(child => MapCategory(child, new HashSet<Guid>(path))).ToList()
                : new List<CategoryDto>();

            return _mapper.Map<CategoryDto>(category) with { Children = children };
        }

        var categoryIds = categories.Select(static x => x.Id).ToHashSet();

        var roots = categories
            .Where(x => !x.ParentId.HasValue || !categoryIds.Contains(x.ParentId.Value))
            .ToList();

        return roots.Select(root => MapCategory(root, new HashSet<Guid>())).ToList();
    }
}
