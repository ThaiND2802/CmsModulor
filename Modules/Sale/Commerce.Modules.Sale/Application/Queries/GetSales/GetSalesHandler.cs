using AutoMapper;
using AutoMapper.QueryableExtensions;
using Commerce.Modules.Sale.Application.DTOs.Responses;
using Commerce.Modules.Sale.Infrastructure;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Sale.Application.Queries.GetSales;

public sealed class GetSalesHandler : IRequestHandler<GetSalesQuery, PagedApiResponse<SaleListItemDto>>
{
    private readonly SaleDbContext _dbContext;
    private readonly IMapper _mapper;

    public GetSalesHandler(SaleDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PagedApiResponse<SaleListItemDto>> Handle(GetSalesQuery query, CancellationToken cancellationToken)
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

        var salesQuery = _dbContext.Sales
            .AsNoTracking()
            .Include(x => x.Items)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            salesQuery = salesQuery.Where(x => x.SaleNumber.Contains(search) || x.CustomerEmail.Contains(search));
        }

        if (query.CustomerId.HasValue)
        {
            salesQuery = salesQuery.Where(x => x.CustomerId == query.CustomerId.Value);
        }

        if (query.Status.HasValue)
        {
            salesQuery = salesQuery.Where(x => x.Status == query.Status.Value);
        }

        salesQuery = query.SortBy?.Trim().ToLowerInvariant() switch
        {
            "sale_number" => query.Desc ? salesQuery.OrderByDescending(x => x.SaleNumber) : salesQuery.OrderBy(x => x.SaleNumber),
            "customer_email" => query.Desc ? salesQuery.OrderByDescending(x => x.CustomerEmail) : salesQuery.OrderBy(x => x.CustomerEmail),
            "total_amount" => query.Desc ? salesQuery.OrderByDescending(x => x.TotalAmount) : salesQuery.OrderBy(x => x.TotalAmount),
            _ => query.Desc ? salesQuery.OrderByDescending(x => x.CreatedAtUtc) : salesQuery.OrderBy(x => x.CreatedAtUtc)
        };

        var total = await salesQuery.CountAsync(cancellationToken);
        var page = query.NormalizedPage;
        var pageSize = query.NormalizedPageSize;
        var sales = await salesQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<SaleListItemDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PagedApiResponse<SaleListItemDto>
        {
            Status = StatusCodes.Status200OK,
            Data = sales,
            Pagination = new PaginationMetadata
            {
                Total = total,
                Page = page,
                PageSize = pageSize
            }
        };
    }
}
