using AutoMapper;
using AutoMapper.QueryableExtensions;
using Commerce.Modules.Inventory.Application.DTOs.Responses;
using Commerce.Modules.Inventory.Infrastructure;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Inventory.Application.Queries.GetStockList;

public sealed class GetStockListHandler : IRequestHandler<GetStockListQuery, PagedApiResponse<InventoryStockDto>>
{
    private readonly InventoryDbContext _dbContext;
    private readonly IMapper _mapper;

    public GetStockListHandler(InventoryDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PagedApiResponse<InventoryStockDto>> Handle(GetStockListQuery query, CancellationToken cancellationToken)
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

        var stockQuery = _dbContext.InventoryItems
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            stockQuery = stockQuery.Where(x => x.Sku.Contains(search));
        }

        stockQuery = (query.SortBy?.ToLowerInvariant(), query.Desc) switch
        {
            ("sku", false) => stockQuery.OrderBy(x => x.Sku),
            ("sku", true) => stockQuery.OrderByDescending(x => x.Sku),
            ("onhandquantity", false) => stockQuery.OrderBy(x => x.OnHandQuantity),
            ("onhandquantity", true) => stockQuery.OrderByDescending(x => x.OnHandQuantity),
            ("availablequantity", false) => stockQuery.OrderBy(x => x.OnHandQuantity - x.ReservedQuantity),
            ("availablequantity", true) => stockQuery.OrderByDescending(x => x.OnHandQuantity - x.ReservedQuantity),
            _ => stockQuery.OrderBy(x => x.Sku)
        };

        var total = await stockQuery.CountAsync(cancellationToken);
        var items = await stockQuery
            .Skip((query.NormalizedPage - 1) * query.NormalizedPageSize)
            .Take(query.NormalizedPageSize)
            .ProjectTo<InventoryStockDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PagedApiResponse<InventoryStockDto>
        {
            Status = StatusCodes.Status200OK,
            Data = items,
            Pagination = new PaginationMetadata
            {
                Total = total,
                Page = query.NormalizedPage,
                PageSize = query.NormalizedPageSize
            }
        };
    }
}
