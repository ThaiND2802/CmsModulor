using Commerce.Modules.Sale.Application.DTOs.Responses;
using Commerce.Modules.Sale.Domain;
using Commerce.Modules.Sale.Infrastructure;
using CommerceCore.SharedKernel.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Commerce.Modules.Sale.Application.Queries.GetSaleTimeline;

public sealed class GetSaleTimelineHandler : IRequestHandler<GetSaleTimelineQuery, SaleTimelineDto>
{
    private readonly SaleDbContext _context;

    public GetSaleTimelineHandler(SaleDbContext context)
    {
        _context = context;
    }

    public async Task<SaleTimelineDto> Handle(GetSaleTimelineQuery request, CancellationToken cancellationToken)
    {
        var sale = await _context.Sales
            .Include(s => s.StatusHistory)
            .FirstOrDefaultAsync(s => s.Id == request.SaleId, cancellationToken);

        if (sale is null)
            throw new NotFoundAppException($"Sale {request.SaleId} not found");

        var events = new List<SaleTimelineEventDto>();

        // Created event
        events.Add(new SaleTimelineEventDto
        {
            OccurredAtUtc = sale.CreatedAtUtc,
            EventType = "SaleCreated",
            Description = "Sale created",
            Actor = sale.CreatedBy
        });

        // Status changes
        foreach (var history in sale.StatusHistory.OrderBy(h => h.ChangedAtUtc))
        {
            if (!TryMapStatusHistoryEvent(history, out var timelineEvent))
            {
                continue;
            }

            events.Add(timelineEvent);
        }

        // Payment completed
        if (sale.PaidAtUtc.HasValue)
        {
            events.Add(new SaleTimelineEventDto
            {
                OccurredAtUtc = sale.PaidAtUtc.Value,
                EventType = "PaymentMarkedPaid",
                Description = $"Payment marked paid: {sale.PaidAmount:C}",
                Actor = sale.UpdatedBy,
                Metadata = new Dictionary<string, object>
                {
                    ["PaidAmount"] = sale.PaidAmount,
                    ["PaymentMethod"] = sale.PaymentMethod.ToString(),
                    ["PaymentReference"] = sale.PaymentReference ?? string.Empty
                }
            });
        }

        return new SaleTimelineDto
        {
            SaleId = sale.Id,
            SaleNumber = sale.SaleNumber,
            Events = events.OrderBy(e => e.OccurredAtUtc).ToList()
        };
    }

    private static bool TryMapStatusHistoryEvent(SaleStatusHistory history, out SaleTimelineEventDto timelineEvent)
    {
        timelineEvent = null!;

        switch (history.ToStatus)
        {
            case Domain.SaleStatus.Priced when history.FromStatus != Domain.SaleStatus.Draft:
                timelineEvent = new SaleTimelineEventDto
                {
                    OccurredAtUtc = history.ChangedAtUtc,
                    EventType = "SaleRepriced",
                    Description = "Sale repriced",
                    Actor = history.ChangedBy
                };
                return true;
            case Domain.SaleStatus.Submitted:
                timelineEvent = new SaleTimelineEventDto
                {
                    OccurredAtUtc = history.ChangedAtUtc,
                    EventType = "SaleSubmitted",
                    Description = "Sale submitted",
                    Actor = history.ChangedBy
                };
                return true;
            case Domain.SaleStatus.Cancelled:
                timelineEvent = new SaleTimelineEventDto
                {
                    OccurredAtUtc = history.ChangedAtUtc,
                    EventType = "SaleCancelled",
                    Description = "Sale cancelled",
                    Actor = history.ChangedBy
                };
                return true;
            default:
                return false;
        }
    }
}
