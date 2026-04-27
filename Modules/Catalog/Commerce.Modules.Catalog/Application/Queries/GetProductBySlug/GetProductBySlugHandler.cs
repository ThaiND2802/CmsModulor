using AutoMapper;
using Commerce.Modules.Catalog.Application.DTOs.Responses;
using Commerce.Modules.Catalog.Infrastructure;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Catalog.Application.Queries.GetProductBySlug;

public sealed class GetProductBySlugHandler : IRequestHandler<GetProductBySlugQuery, ProductDto>
{
    private readonly CatalogDbContext _dbContext;
    private readonly IMapper _mapper;

    public GetProductBySlugHandler(CatalogDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<ProductDto> Handle(GetProductBySlugQuery query, CancellationToken cancellationToken)
    {
        var slug = query.Slug?.Trim();
        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new NotFoundAppException("Product slug was not provided.");
        }

        var product = await _dbContext.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Brand)
            .Include(x => x.Media)
            .Include(x => x.AttributeValues)
                .ThenInclude(x => x.Attribute)
            .Include(x => x.Variants)
                .ThenInclude(x => x.Options)
                .ThenInclude(x => x.Attribute)
            .SingleOrDefaultAsync(x => x.Slug == slug, cancellationToken)
            ?? throw new NotFoundAppException($"Product with slug '{slug}' was not found.");

        return _mapper.Map<ProductDto>(product);
    }
}
