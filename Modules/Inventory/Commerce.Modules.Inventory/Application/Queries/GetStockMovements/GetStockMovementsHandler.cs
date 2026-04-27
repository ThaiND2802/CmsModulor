using AutoMapper;
using AutoMapper.QueryableExtensions;
using Commerce.Modules.Inventory.Application.DTOs.Responses;
using Commerce.Modules.Inventory.Infrastructure;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Inventory.Application.Queries.GetStockMovements;

public sealed class GetStockMovementsHandler : IRequestHandler<GetStockMovementsQuery, PagedApiResponse<StockMovementDto>>
{
    private readonly InventoryDbContext _dbContext;
    private readonly IMapper _mapper;

    public GetStockMovementsHandler(InventoryDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PagedApiResponse<StockMovementDto>> Handle(GetStockMovementsQuery query, CancellationToken cancellationToken)
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

        var inventoryItemExists = await _dbContext.InventoryItems
            .AsNoTracking()
            .AnyAsync(x => x.VariantId == query.VariantId, cancellationToken);

        if (!inventoryItemExists)
        {
            throw new NotFoundAppException($"Inventory item for variant '{query.VariantId}' was not found.");
        }

        var movementQuery = _dbContext.StockMovements
            .AsNoTracking()
            .Include(x => x.InventoryItem)
            .Where(x => x.InventoryItem.VariantId == query.VariantId);

        var total = await movementQuery.CountAsync(cancellationToken);
        var items = await movementQuery
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((query.NormalizedPage - 1) * query.NormalizedPageSize)
            .Take(query.NormalizedPageSize)
            .ProjectTo<StockMovementDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new PagedApiResponse<StockMovementDto>
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
