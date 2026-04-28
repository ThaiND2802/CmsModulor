using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommerceCore.Application.Abstractions;

namespace Commerce.Modules.Sale.Domain;

[Table("sale_sales")]
public sealed class Sale : IAuditableEntity, ISoftDelete
{
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("sale_number")]
    [MaxLength(50)]
    public string SaleNumber { get; set; } = string.Empty;

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
    public SaleStatus Status { get; set; } = SaleStatus.Draft;

    public SaleAddress? ShippingAddress { get; set; }

    public SaleAddress? BillingAddress { get; set; }

    [Column("subtotal_amount")]
    public decimal SubtotalAmount { get; set; }

    [Column("discount_amount")]
    public decimal DiscountAmount { get; set; }

    [Column("base_discount_amount")]
    public decimal BaseDiscountAmount { get; set; }

    [Column("coupon_discount_amount")]
    public decimal CouponDiscountAmount { get; set; }

    [Column("shipping_amount")]
    public decimal ShippingAmount { get; set; }

    [Column("base_shipping_amount")]
    public decimal BaseShippingAmount { get; set; }

    [Column("coupon_shipping_discount_amount")]
    public decimal CouponShippingDiscountAmount { get; set; }

    [Column("tax_amount")]
    public decimal TaxAmount { get; set; }

    [Column("total_amount")]
    public decimal TotalAmount { get; set; }

    [Required]
    [Column("currency")]
    [MaxLength(3)]
    public string Currency { get; set; } = string.Empty;

    [Column("notes")]
    [MaxLength(2000)]
    public string? Notes { get; set; }

    [Column("submitted_at_utc")]
    public DateTime? SubmittedAtUtc { get; set; }

    [Column("expires_at_utc")]
    public DateTime? ExpiresAtUtc { get; set; }

    [Column("coupon_code")]
    [MaxLength(100)]
    public string? CouponCode { get; set; }

    [Column("coupon_type")]
    [MaxLength(50)]
    public string? CouponType { get; set; }

    [Column("coupon_value")]
    public decimal? CouponValue { get; set; }

    [Column("order_id")]
    public Guid? OrderId { get; set; }

    [Column("channel")]
    public SaleChannel Channel { get; set; } = SaleChannel.Web;

    [Column("payment_method")]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.None;

    [Column("payment_status")]
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

    [Column("paid_amount")]
    public decimal PaidAmount { get; set; }

    [Column("payment_reference")]
    [MaxLength(200)]
    public string? PaymentReference { get; set; }

    [Column("payment_initiated_at_utc")]
    public DateTime? PaymentInitiatedAtUtc { get; set; }

    [Column("paid_at_utc")]
    public DateTime? PaidAtUtc { get; set; }

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

    public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();

    public ICollection<SaleStatusHistory> StatusHistory { get; set; } = new List<SaleStatusHistory>();

    public bool IsMutable() => Status is SaleStatus.Draft or SaleStatus.Priced;

    public bool IsExpired(DateTime utcNow) =>
        Status == SaleStatus.Expired || (ExpiresAtUtc.HasValue && ExpiresAtUtc.Value <= utcNow);

    public bool CanSubmit() => Status == SaleStatus.Priced && OrderId is null;

    public bool CanCancel() => Status is SaleStatus.Draft or SaleStatus.Priced;

    public void SelectPaymentMethod(PaymentMethod method)
    {
        if (Status != SaleStatus.Priced)
            throw new InvalidOperationException($"Cannot select payment method in {Status} status");

        PaymentMethod = method;
    }

    public void MarkPaid(decimal amount, string? reference = null)
    {
        if (PaymentStatus == PaymentStatus.Paid)
            return; // Idempotent

        if (Status != SaleStatus.Priced)
            throw new InvalidOperationException($"Cannot mark paid in {Status} status");

        PaymentStatus = PaymentStatus.Paid;
        PaidAmount = amount;
        if (!string.IsNullOrWhiteSpace(reference))
            PaymentReference = reference;
        PaidAtUtc = DateTime.UtcNow;
    }
}
