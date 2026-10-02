using MediCareX.Domain.Common;

namespace MediCareX.Domain.Entities;

public class LabTest : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? Category { get; set; }
    public string? PreparationInstructions { get; set; }
    public int TurnaroundHours { get; set; } = 24;
}
