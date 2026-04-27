using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommerceCore.Application.Abstractions;

namespace Commerce.Modules.Order.Domain;

[Table("order_orders")]
public sealed class Order : IAuditableEntity, ISoftDelete
{
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("order_number")]
    [MaxLength(50)]
    public string OrderNumber { get; set; } = string.Empty;

    [Column("customer_id")]
    public Guid? CustomerId { get; set; }

    [Required]
    [Column("customer_email")]
    [MaxLength(256)]
    public string CustomerEmail { get; set; } = string.Empty;

    [Column("customer_phone")]
    [MaxLength(50)]
    public string? CustomerPhone { get; set; }

    [Column("status")]
    public OrderStatus Status { get; set; }

    [Column("notes")]
    [MaxLength(2000)]
    public string? Notes { get; set; }

    public OrderAddress ShippingAddress { get; set; } = new();

    public OrderAddress? BillingAddress { get; set; }

    [Column("subtotal_amount")]
    public decimal SubtotalAmount { get; set; }

    [Column("discount_amount")]
    public decimal DiscountAmount { get; set; }

    [Column("shipping_amount")]
    public decimal ShippingAmount { get; set; }

    [Column("tax_amount")]
    public decimal TaxAmount { get; set; }

    [Column("total_amount")]
    public decimal TotalAmount { get; set; }

    [Required]
    [Column("currency")]
    [MaxLength(3)]
    public string Currency { get; set; } = string.Empty;

    [Column("cancel_reason")]
    [MaxLength(1000)]
    public string? CancelReason { get; set; }

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

    [Column("is_deleted")]
    public bool IsDeleted { get; set; }

    [Column("deleted_at_utc")]
    public DateTime? DeletedAtUtc { get; set; }

    [Column("deleted_by")]
    [MaxLength(100)]
    public string? DeletedBy { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
}
