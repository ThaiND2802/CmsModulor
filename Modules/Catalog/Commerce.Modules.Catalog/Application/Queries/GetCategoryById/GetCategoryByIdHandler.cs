using AutoMapper;
using Commerce.Modules.Catalog.Application.DTOs.Responses;
using Commerce.Modules.Catalog.Infrastructure;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Catalog.Application.Queries.GetCategoryById;

public sealed class GetCategoryByIdHandler : IRequestHandler<GetCategoryByIdQuery, CategoryDto>
{
    private readonly CatalogDbContext _dbContext;
    private readonly IMapper _mapper;

    public GetCategoryByIdHandler(CatalogDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<CategoryDto> Handle(GetCategoryByIdQuery query, CancellationToken cancellationToken)
    {
        var category = await _dbContext.Categories
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == query.Id, cancellationToken)
            ?? throw new NotFoundAppException($"Category '{query.Id}' was not found.");

        return _mapper.Map<CategoryDto>(category) with { Children = Array.Empty<CategoryDto>() };
    }
}
