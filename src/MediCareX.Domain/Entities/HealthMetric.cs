using MediCareX.Domain.Common;
using MediCareX.Domain.Enums;

namespace MediCareX.Domain.Entities;

/// <summary>A single reading (BP uses Value=systolic, SecondaryValue=diastolic).</summary>
public class HealthMetric : BaseEntity
{
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public HealthMetricType Type { get; set; }
    public decimal Value { get; set; }
    public decimal? SecondaryValue { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
}
