using AutoMapper;
using Commerce.Modules.Catalog.Application.DTOs.Responses;
using Commerce.Modules.Catalog.Infrastructure;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Catalog.Application.Queries.GetBrandById;

public sealed class GetBrandByIdHandler : IRequestHandler<GetBrandByIdQuery, BrandDto>
{
    private readonly CatalogDbContext _dbContext;
    private readonly IMapper _mapper;

    public GetBrandByIdHandler(CatalogDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<BrandDto> Handle(GetBrandByIdQuery query, CancellationToken cancellationToken)
    {
        var brand = await _dbContext.Brands
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == query.Id, cancellationToken)
            ?? throw new NotFoundAppException($"Brand '{query.Id}' was not found.");

        return _mapper.Map<BrandDto>(brand);
    }
}
