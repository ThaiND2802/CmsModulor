using Commerce.Modules.Sale.Application.DTOs.Responses;
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
            events.Add(new SaleTimelineEventDto
            {
                OccurredAtUtc = history.ChangedAtUtc,
                EventType = "StatusChanged",
                Description = $"Status changed from {history.FromStatus} to {history.ToStatus}",
                Actor = history.ChangedBy,
                Metadata = new Dictionary<string, object>
                {
                    ["FromStatus"] = history.FromStatus.ToString(),
                    ["ToStatus"] = history.ToStatus.ToString()
                }
            });
        }

        // Coupon applied
        if (!string.IsNullOrWhiteSpace(sale.CouponCode))
        {
            events.Add(new SaleTimelineEventDto
            {
                OccurredAtUtc = sale.UpdatedAtUtc ?? sale.CreatedAtUtc,
                EventType = "CouponApplied",
                Description = $"Coupon '{sale.CouponCode}' applied",
                Actor = sale.UpdatedBy,
                Metadata = new Dictionary<string, object>
                {
                    ["CouponCode"] = sale.CouponCode,
                    ["CouponType"] = sale.CouponType ?? string.Empty,
                    ["DiscountAmount"] = sale.CouponDiscountAmount
                }
            });
        }

        // Payment initiated
        if (sale.PaymentInitiatedAtUtc.HasValue)
        {
            events.Add(new SaleTimelineEventDto
            {
                OccurredAtUtc = sale.PaymentInitiatedAtUtc.Value,
                EventType = "PaymentInitiated",
                Description = $"Payment initiated via {sale.PaymentMethod}",
                Actor = sale.UpdatedBy,
                Metadata = new Dictionary<string, object>
                {
                    ["PaymentMethod"] = sale.PaymentMethod.ToString(),
                    ["PaymentReference"] = sale.PaymentReference ?? string.Empty
                }
            });
        }

        // Payment completed
        if (sale.PaidAtUtc.HasValue)
        {
            events.Add(new SaleTimelineEventDto
            {
                OccurredAtUtc = sale.PaidAtUtc.Value,
                EventType = "PaymentCompleted",
                Description = $"Payment completed: {sale.PaidAmount:C}",
                Actor = sale.UpdatedBy,
                Metadata = new Dictionary<string, object>
                {
                    ["PaidAmount"] = sale.PaidAmount,
                    ["PaymentReference"] = sale.PaymentReference ?? string.Empty
                }
            });
        }

        // Submitted
        if (sale.SubmittedAtUtc.HasValue)
        {
            events.Add(new SaleTimelineEventDto
            {
                OccurredAtUtc = sale.SubmittedAtUtc.Value,
                EventType = "SaleSubmitted",
                Description = "Sale submitted",
                Actor = sale.UpdatedBy,
                Metadata = new Dictionary<string, object>
                {
                    ["OrderId"] = sale.OrderId?.ToString() ?? string.Empty
                }
            });
        }

        // Expired
        if (sale.ExpiresAtUtc.HasValue && sale.Status == Domain.SaleStatus.Expired)
        {
            events.Add(new SaleTimelineEventDto
            {
                OccurredAtUtc = sale.ExpiresAtUtc.Value,
                EventType = "SaleExpired",
                Description = "Sale expired",
                Actor = "System"
            });
        }

        return new SaleTimelineDto
        {
            SaleId = sale.Id,
            SaleNumber = sale.SaleNumber,
            Events = events.OrderBy(e => e.OccurredAtUtc).ToList()
        };
    }
}
