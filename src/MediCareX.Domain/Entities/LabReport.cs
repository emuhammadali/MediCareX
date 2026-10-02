using MediCareX.Domain.Common;
using MediCareX.Domain.Enums;

namespace MediCareX.Domain.Entities;

/// <summary>A lab order and its eventual result.</summary>
public class LabReport : BaseEntity
{
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public Guid TestId { get; set; }
    public LabTest Test { get; set; } = null!;
    public Guid? OrderedByDoctorId { get; set; }
    public Doctor? OrderedByDoctor { get; set; }
    public Guid? LabTechnicianId { get; set; }

    public LabTestStatus Status { get; set; } = LabTestStatus.Booked;
    public DateTime? SampleScheduledAt { get; set; }
    public DateTime? SampleCollectedAt { get; set; }
    public string? CollectionAddress { get; set; }
    public string? ReportUrl { get; set; }
    /// <summary>Parsed numeric results as JSON: {"Hemoglobin": 13.2, ...}. Used for trend comparison.</summary>
    public string? ResultsJson { get; set; }
    public string? OcrText { get; set; }
    public DateTime? ReportedAt { get; set; }
}
