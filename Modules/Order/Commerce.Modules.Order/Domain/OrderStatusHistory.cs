using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Commerce.Modules.Order.Domain;

[Table("order_status_history")]
public sealed class OrderStatusHistory
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("order_id")]
    public Guid OrderId { get; set; }

    [Column("from_status")]
    public OrderStatus? FromStatus { get; set; }

    [Column("to_status")]
    public OrderStatus ToStatus { get; set; }

    [Column("note")]
    [MaxLength(1000)]
    public string? Note { get; set; }

    [Column("changed_by")]
    [MaxLength(100)]
    public string? ChangedBy { get; set; }

    [Column("changed_at_utc")]
    public DateTime ChangedAtUtc { get; set; }

    public Order Order { get; set; } = null!;
}
