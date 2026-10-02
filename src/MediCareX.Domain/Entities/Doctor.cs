using MediCareX.Domain.Common;

namespace MediCareX.Domain.Entities;

public class Doctor : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public string Specialization { get; set; } = string.Empty;
    public string Qualifications { get; set; } = string.Empty;
    public int ExperienceYears { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public decimal ConsultationFee { get; set; }
    public string Currency { get; set; } = "PKR";
    public double Rating { get; set; }
    public int RatingCount { get; set; }
    public int TotalConsultations { get; set; }
    public string? Bio { get; set; }
    public bool IsApproved { get; set; }
    public DateTime? ApprovedAt { get; set; }
    /// <summary>Percentage the platform keeps from each consultation.</summary>
    public decimal CommissionPercent { get; set; } = 20m;
    public string? SignatureImageUrl { get; set; }
    public string? GoogleCalendarId { get; set; }

    public ICollection<AvailabilitySlot> AvailabilitySlots { get; set; } = new List<AvailabilitySlot>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}

/// <summary>Weekly recurring availability window.</summary>
public class AvailabilitySlot : BaseEntity
{
    public Guid DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int SlotDurationMinutes { get; set; } = 30;
    public bool IsActive { get; set; } = true;
}
