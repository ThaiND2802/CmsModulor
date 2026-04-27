using AutoMapper;
using AutoMapper.QueryableExtensions;
using Commerce.Modules.Catalog.Application.DTOs.Responses;
using Commerce.Modules.Catalog.Infrastructure;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Catalog.Application.Queries.GetBrands;

public sealed class GetBrandsHandler : IRequestHandler<GetBrandsQuery, PagedApiResponse<BrandDto>>
{
    private readonly CatalogDbContext _dbContext;
    private readonly IMapper _mapper;

    public GetBrandsHandler(CatalogDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PagedApiResponse<BrandDto>> Handle(GetBrandsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.Page <= 0)
        {
            throw new ValidationAppException("Page must be greater than 0.");
        }

        if (query.PageSize <= 0)
        {
            throw new ValidationAppException("Page size must be greater than 0.");
        }

        var brandsQuery = _dbContext.Brands.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            brandsQuery = brandsQuery.Where(x => x.Name.Contains(search));
        }

        brandsQuery = brandsQuery.OrderBy(static x => x.Name);

        var total = await brandsQuery.CountAsync(cancellationToken);
        var page = query.NormalizedPage;
        var pageSize = query.NormalizedPageSize;
        var brands = await brandsQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<BrandDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PagedApiResponse<BrandDto>
        {
            Status = StatusCodes.Status200OK,
            Data = brands,
            Pagination = new PaginationMetadata
            {
                Total = total,
                Page = page,
                PageSize = pageSize
            }
        };
    }
}
