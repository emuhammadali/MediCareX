using MediCareX.Domain.Common;

namespace MediCareX.Domain.Entities;

public class Medicine : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string GenericName { get; set; } = string.Empty;
    public string? Manufacturer { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int ReorderLevel { get; set; } = 10;
    public string? Category { get; set; }
    public bool RequiresPrescription { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsLowStock => Stock <= ReorderLevel;
}
