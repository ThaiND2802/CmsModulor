using AutoMapper;
using Commerce.Modules.Inventory.Application.DTOs.Responses;
using Commerce.Modules.Inventory.Infrastructure;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Inventory.Application.Queries.GetStockByVariant;

public sealed class GetStockByVariantHandler : IRequestHandler<GetStockByVariantQuery, ApiResponse<InventoryStockDto>>
{
    private readonly InventoryDbContext _dbContext;
    private readonly IMapper _mapper;

    public GetStockByVariantHandler(InventoryDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<ApiResponse<InventoryStockDto>> Handle(GetStockByVariantQuery query, CancellationToken cancellationToken)
    {
        var inventoryItem = await _dbContext.InventoryItems
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.VariantId == query.VariantId, cancellationToken);

        if (inventoryItem is null)
        {
            throw new NotFoundAppException($"Inventory item for variant '{query.VariantId}' was not found.");
        }

        return new ApiResponse<InventoryStockDto>
        {
            Status = StatusCodes.Status200OK,
            Data = _mapper.Map<InventoryStockDto>(inventoryItem)
        };
    }
}
