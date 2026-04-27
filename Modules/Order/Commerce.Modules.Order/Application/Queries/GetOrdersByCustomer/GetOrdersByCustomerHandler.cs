using AutoMapper;
using Commerce.Modules.Order.Application.DTOs.Responses;
using Commerce.Modules.Order.Infrastructure;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Order.Application.Queries.GetOrdersByCustomer;

public sealed class GetOrdersByCustomerHandler : IRequestHandler<GetOrdersByCustomerQuery, PagedApiResponse<OrderListItemDto>>
{
    private readonly OrderDbContext _dbContext;
    private readonly IMapper _mapper;

    public GetOrdersByCustomerHandler(OrderDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PagedApiResponse<OrderListItemDto>> Handle(GetOrdersByCustomerQuery query, CancellationToken cancellationToken)
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

        var ordersQuery = _dbContext.Orders
            .Include(static x => x.Items)
            .Where(x => x.CustomerId == query.CustomerId)
            .OrderByDescending(static x => x.CreatedAtUtc);

        var totalCount = await ordersQuery.CountAsync(cancellationToken);
        var orders = await ordersQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedApiResponse<OrderListItemDto>
        {
            Status = StatusCodes.Status200OK,
            Data = orders.Select(_mapper.Map<OrderListItemDto>).ToList(),
            Pagination = new PaginationMetadata
            {
                Total = totalCount,
                Page = query.Page,
                PageSize = query.PageSize
            }
        };
    }
}
