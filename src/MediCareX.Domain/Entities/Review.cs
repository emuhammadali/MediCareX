using MediCareX.Domain.Common;

namespace MediCareX.Domain.Entities;

public class Review : BaseEntity
{
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public Guid DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
    public Guid AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;
    /// <summary>1–5.</summary>
    public int Rating { get; set; }
    public string? Comment { get; set; }
}
