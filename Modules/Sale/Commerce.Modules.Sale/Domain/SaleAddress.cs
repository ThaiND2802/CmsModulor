using System.ComponentModel.DataAnnotations;

namespace Commerce.Modules.Sale.Domain;

public sealed class SaleAddress
{
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string PhoneNumber { get; set; } = string.Empty;

    [MaxLength(500)]
    public string AddressLine1 { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? AddressLine2 { get; set; }

    [MaxLength(200)]
    public string City { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? State { get; set; }

    [MaxLength(20)]
    public string? PostalCode { get; set; }

    [MaxLength(100)]
    public string Country { get; set; } = string.Empty;
}
