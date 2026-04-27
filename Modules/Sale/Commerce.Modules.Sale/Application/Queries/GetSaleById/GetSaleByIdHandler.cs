using AutoMapper;
using Commerce.Modules.Sale.Application.DTOs.Responses;
using Commerce.Modules.Sale.Infrastructure;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Sale.Application.Queries.GetSaleById;

public sealed class GetSaleByIdHandler : IRequestHandler<GetSaleByIdQuery, SaleDto>
{
    private readonly SaleDbContext _dbContext;
    private readonly IMapper _mapper;

    public GetSaleByIdHandler(SaleDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<SaleDto> Handle(GetSaleByIdQuery query, CancellationToken cancellationToken)
    {
        var sale = await _dbContext.Sales
            .AsNoTracking()
            .Include(x => x.Items)
            .Include(x => x.StatusHistory)
            .FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken)
            ?? throw new NotFoundAppException($"Sale '{query.Id}' was not found.");

        return _mapper.Map<SaleDto>(sale);
    }
}
