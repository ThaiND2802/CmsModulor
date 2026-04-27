using AutoMapper;
using Commerce.Modules.Order.Application.DTOs.Responses;
using Commerce.Modules.Order.Infrastructure;
using CommerceCore.Application.Responses;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Order.Application.Commands.UpdateOrderNotes;

public sealed class UpdateOrderNotesHandler : IRequestHandler<UpdateOrderNotesCommand, ApiResponse<OrderDto>>
{
    private readonly OrderDbContext _dbContext;
    private readonly IMapper _mapper;

    public UpdateOrderNotesHandler(OrderDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<ApiResponse<OrderDto>> Handle(UpdateOrderNotesCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var order = await _dbContext.Orders
            .Include(static x => x.Items)
            .Include(static x => x.StatusHistory)
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken)
            ?? throw new NotFoundAppException($"Order '{command.Id}' was not found.");

        order.Notes = string.IsNullOrWhiteSpace(command.Notes) ? null : command.Notes.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ApiResponse<OrderDto>
        {
            Status = StatusCodes.Status200OK,
            Data = _mapper.Map<OrderDto>(order)
        };
    }
}
