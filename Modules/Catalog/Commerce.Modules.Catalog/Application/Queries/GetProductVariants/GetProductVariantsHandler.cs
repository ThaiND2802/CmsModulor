using AutoMapper;
using Commerce.Modules.Catalog.Application.DTOs.Responses;
using Commerce.Modules.Catalog.Infrastructure;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Catalog.Application.Queries.GetProductVariants;

public sealed class GetProductVariantsHandler : IRequestHandler<GetProductVariantsQuery, IReadOnlyList<ProductVariantDto>>
{
    private readonly CatalogDbContext _dbContext;
    private readonly IMapper _mapper;

    public GetProductVariantsHandler(CatalogDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<IReadOnlyList<ProductVariantDto>> Handle(GetProductVariantsQuery query, CancellationToken cancellationToken)
    {
        var productExists = await _dbContext.Products
            .AsNoTracking()
            .AnyAsync(x => x.Id == query.ProductId, cancellationToken);

        if (!productExists)
        {
            throw new NotFoundAppException($"Product '{query.ProductId}' was not found.");
        }

        var variants = await _dbContext.ProductVariants
            .AsNoTracking()
            .Where(x => x.ProductId == query.ProductId)
            .Include(x => x.Options)
                .ThenInclude(x => x.Attribute)
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return variants.Select(_mapper.Map<ProductVariantDto>).ToList();
    }
}
