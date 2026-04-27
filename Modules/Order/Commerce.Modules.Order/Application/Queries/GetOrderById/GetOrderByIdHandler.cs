using AutoMapper;
using Commerce.Modules.Order.Application.DTOs.Responses;
using Commerce.Modules.Order.Infrastructure;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Order.Application.Queries.GetOrderById;

public sealed class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, ApiResponse<OrderDto>>
{
    private readonly OrderDbContext _dbContext;
    private readonly IMapper _mapper;

    public GetOrderByIdHandler(OrderDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<ApiResponse<OrderDto>> Handle(GetOrderByIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var order = await _dbContext.Orders
            .Include(static x => x.Items)
            .Include(static x => x.StatusHistory)
            .FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken)
            ?? throw new NotFoundAppException($"Order '{query.Id}' was not found.");

        return new ApiResponse<OrderDto>
        {
            Status = StatusCodes.Status200OK,
            Data = _mapper.Map<OrderDto>(order)
        };
    }
}
