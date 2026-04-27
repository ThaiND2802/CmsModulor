using AutoMapper;
using Commerce.Modules.Order.Application.DTOs.Responses;
using Commerce.Modules.Order.Infrastructure;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Order.Application.Queries.GetOrders;

public sealed class GetOrdersHandler : IRequestHandler<GetOrdersQuery, PagedApiResponse<OrderListItemDto>>
{
    private readonly OrderDbContext _dbContext;
    private readonly IMapper _mapper;

    public GetOrdersHandler(OrderDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PagedApiResponse<OrderListItemDto>> Handle(GetOrdersQuery query, CancellationToken cancellationToken)
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
            .Include(static o => o.Items)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            ordersQuery = ordersQuery.Where(o =>
                o.OrderNumber.Contains(search) ||
                o.CustomerEmail.Contains(search));
        }

        if (query.CustomerId.HasValue)
        {
            var customerId = query.CustomerId.Value;
            ordersQuery = ordersQuery.Where(o => o.CustomerId == customerId);
        }

        if (query.Status.HasValue)
        {
            var status = query.Status.Value;
            ordersQuery = ordersQuery.Where(o => o.Status == status);
        }

        if (query.FromDate.HasValue)
        {
            var fromDate = query.FromDate.Value;
            ordersQuery = ordersQuery.Where(o => o.CreatedAtUtc >= fromDate);
        }

        if (query.ToDate.HasValue)
        {
            var toDate = query.ToDate.Value;
            ordersQuery = ordersQuery.Where(o => o.CreatedAtUtc <= toDate);
        }

        ordersQuery = ApplySorting(ordersQuery, query);

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

    private static IQueryable<Commerce.Modules.Order.Domain.Order> ApplySorting(
        IQueryable<Commerce.Modules.Order.Domain.Order> ordersQuery,
        GetOrdersQuery query)
    {
        return query.SortBy?.Trim() switch
        {
            "createdAt" => query.SortDescending
                ? ordersQuery.OrderByDescending(static o => o.CreatedAtUtc)
                : ordersQuery.OrderBy(static o => o.CreatedAtUtc),
            "totalAmount" => query.SortDescending
                ? ordersQuery.OrderByDescending(static o => o.TotalAmount)
                : ordersQuery.OrderBy(static o => o.TotalAmount),
            "status" => query.SortDescending
                ? ordersQuery.OrderByDescending(static o => o.Status)
                : ordersQuery.OrderBy(static o => o.Status),
            _ => ordersQuery.OrderByDescending(static o => o.CreatedAtUtc)
        };
    }
}
