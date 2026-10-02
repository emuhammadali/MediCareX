using MediCareX.Domain.Common;
using MediCareX.Domain.Enums;
using MediCareX.Domain.ValueObjects;

namespace MediCareX.Domain.Entities;

public class Patient : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    /// <summary>13 digits, no dashes. Encrypt at rest (value converter in Infrastructure).</summary>
    public string? Cnic { get; set; }
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public BloodGroup BloodGroup { get; set; }
    public string? Allergies { get; set; }
    public string? ChronicConditions { get; set; }
    public string? MedicalHistorySummary { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public Address Address { get; set; } = Address.Empty;
    public SubscriptionPlan Plan { get; set; } = SubscriptionPlan.None;
    public DateTime? PlanExpiresAt { get; set; }

    public Guid? WalletId { get; set; }
    public Wallet? Wallet { get; set; }

    /// <summary>Self-reference: dependents (children, parents) managed by this patient.</summary>
    public Guid? GuardianPatientId { get; set; }
    public Patient? GuardianPatient { get; set; }
    public string? RelationToGuardian { get; set; }
    public ICollection<Patient> Dependents { get; set; } = new List<Patient>();

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    public ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
    public ICollection<LabReport> LabReports { get; set; } = new List<LabReport>();
    public ICollection<HealthMetric> HealthMetrics { get; set; } = new List<HealthMetric>();

    public int Age => (int)((DateTime.UtcNow - DateOfBirth).TotalDays / 365.25);
}
