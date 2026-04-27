using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommerceCore.Application.Abstractions;

namespace Commerce.Modules.Inventory.Domain;

[Table("inventory_stock_reservations")]
public sealed class StockReservation : IAuditableEntity
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("order_id")]
    public Guid OrderId { get; set; }

    [Column("status")]
    public StockReservationStatus Status { get; set; }

    [Column("created_at_utc")]
    public DateTime CreatedAtUtc { get; set; }

    [Column("created_by")]
    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    [Column("updated_at_utc")]
    public DateTime? UpdatedAtUtc { get; set; }

    [Column("updated_by")]
    [MaxLength(100)]
    public string? UpdatedBy { get; set; }

    [Column("released_at_utc")]
    public DateTime? ReleasedAtUtc { get; set; }

    public ICollection<StockReservationItem> Items { get; set; } = new List<StockReservationItem>();

    public static StockReservation Create(Guid orderId)
    {
        if (orderId == Guid.Empty)
        {
            throw new ArgumentException("Order ID is required.", nameof(orderId));
        }

        return new StockReservation
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Status = StockReservationStatus.Active
        };
    }

    public void Release(DateTime releasedAtUtc)
    {
        if (Status == StockReservationStatus.Released)
        {
            throw new InvalidOperationException("Reservation has already been released.");
        }

        Status = StockReservationStatus.Released;
        ReleasedAtUtc = releasedAtUtc;
    }
}
