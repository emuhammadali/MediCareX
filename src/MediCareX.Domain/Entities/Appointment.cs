using MediCareX.Domain.Common;
using MediCareX.Domain.Enums;

namespace MediCareX.Domain.Entities;

public class Appointment : BaseEntity
{
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public Guid DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;

    public DateTime ScheduledAt { get; set; }
    public int DurationMinutes { get; set; } = 30;
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    public AppointmentType Type { get; set; } = AppointmentType.Online;
    public string? Symptoms { get; set; }
    public string? Notes { get; set; }
    public string? RejectionReason { get; set; }
    public string? ConsultationSummary { get; set; }
    public string? GoogleCalendarEventId { get; set; }
    public bool ReminderSent { get; set; }
    public decimal Fee { get; set; }

    public Guid? PaymentId { get; set; }
    public Payment? Payment { get; set; }
    public Guid? VideoSessionId { get; set; }
    public VideoSession? VideoSession { get; set; }
    public Prescription? Prescription { get; set; }
    public Review? Review { get; set; }

    public DateTime EndsAt => ScheduledAt.AddMinutes(DurationMinutes);
}
