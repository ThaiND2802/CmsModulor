using AutoMapper;
using AutoMapper.QueryableExtensions;
using Commerce.Modules.Catalog.Application.DTOs.Responses;
using Commerce.Modules.Catalog.Infrastructure;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Catalog.Application.Queries.GetProducts;

public sealed class GetProductsHandler : IRequestHandler<GetProductsQuery, PagedApiResponse<ProductListItemDto>>
{
    private readonly CatalogDbContext _dbContext;
    private readonly IMapper _mapper;

    public GetProductsHandler(CatalogDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PagedApiResponse<ProductListItemDto>> Handle(GetProductsQuery query, CancellationToken cancellationToken)
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

        var productsQuery = _dbContext.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Brand)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            productsQuery = productsQuery.Where(x => x.Name.Contains(search) || x.Sku.Contains(search));
        }

        if (query.CategoryId.HasValue)
        {
            productsQuery = productsQuery.Where(x => x.CategoryId == query.CategoryId.Value);
        }

        if (query.BrandId.HasValue)
        {
            productsQuery = productsQuery.Where(x => x.BrandId == query.BrandId.Value);
        }

        if (query.Status.HasValue)
        {
            productsQuery = productsQuery.Where(x => x.Status == query.Status.Value);
        }

        if (query.IsFeatured.HasValue)
        {
            productsQuery = productsQuery.Where(x => x.IsFeatured == query.IsFeatured.Value);
        }

        productsQuery = (query.SortBy?.ToLowerInvariant(), query.Desc) switch
        {
            ("name", false) => productsQuery.OrderBy(x => x.Name),
            ("name", true) => productsQuery.OrderByDescending(x => x.Name),
            ("sku", false) => productsQuery.OrderBy(x => x.Sku),
            ("sku", true) => productsQuery.OrderByDescending(x => x.Sku),
            ("createdatutc", false) => productsQuery.OrderBy(x => x.CreatedAtUtc),
            _ => productsQuery.OrderByDescending(x => x.CreatedAtUtc)
        };

        var total = await productsQuery.CountAsync(cancellationToken);
        var page = query.NormalizedPage;
        var pageSize = query.NormalizedPageSize;
        var products = await productsQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<ProductListItemDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PagedApiResponse<ProductListItemDto>
        {
            Status = StatusCodes.Status200OK,
            Data = products,
            Pagination = new PaginationMetadata
            {
                Total = total,
                Page = page,
                PageSize = pageSize
            }
        };
    }
}
