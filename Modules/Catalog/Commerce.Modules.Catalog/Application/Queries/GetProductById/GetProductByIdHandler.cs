using AutoMapper;
using Commerce.Modules.Catalog.Application.DTOs.Responses;
using Commerce.Modules.Catalog.Infrastructure;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Catalog.Application.Queries.GetProductById;

public sealed class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    private readonly CatalogDbContext _dbContext;
    private readonly IMapper _mapper;

    public GetProductByIdHandler(CatalogDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<ProductDto> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
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
            .SingleOrDefaultAsync(x => x.Id == query.Id, cancellationToken)
            ?? throw new NotFoundAppException($"Product '{query.Id}' was not found.");

        return _mapper.Map<ProductDto>(product);
    }
}
