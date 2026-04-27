using AutoMapper;
using Commerce.Modules.Order.Application.DTOs.Responses;
using Commerce.Modules.Order.Infrastructure;
using CommerceCore.Application.Abstractions;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Order.Application.Queries.GetMyOrders;

public sealed class GetMyOrdersHandler : IRequestHandler<GetMyOrdersQuery, PagedApiResponse<OrderListItemDto>>
{
    private readonly OrderDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly ICurrentUser _currentUser;

    public GetMyOrdersHandler(OrderDbContext dbContext, IMapper mapper, ICurrentUser currentUser)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<PagedApiResponse<OrderListItemDto>> Handle(GetMyOrdersQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (!_currentUser.IsAuthenticated || !Guid.TryParse(_currentUser.UserId, out var userId))
        {
            throw new UnauthorizedAppException("The current user is not authenticated.");
        }

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
            .Where(x => x.CustomerId == userId)
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
