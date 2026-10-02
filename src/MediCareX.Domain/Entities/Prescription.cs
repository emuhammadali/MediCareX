using MediCareX.Domain.Common;

namespace MediCareX.Domain.Entities;

/// <summary>One line of a prescription. Persisted as a JSON column.</summary>
public record PrescribedMedicine(string Name, string Dosage, string Frequency, int DurationDays, string? Instructions, Guid? MedicineId = null);

public class Prescription : BaseEntity
{
    public Guid AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;
    public Guid DoctorId { get; set; }
    public Doctor Doctor { get; set; } = null!;
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public string Diagnosis { get; set; } = string.Empty;
    public List<PrescribedMedicine> Medicines { get; set; } = new();
    public string? Instructions { get; set; }
    public string? RecommendedLabTests { get; set; }
    public DateTime ValidUntil { get; set; }

    /// <summary>Opaque verification code embedded in the QR (never the raw Id).</summary>
    public string VerificationCode { get; set; } = Convert.ToHexString(Guid.NewGuid().ToByteArray());
    public string? QrCodeUrl { get; set; }
    public string? PdfUrl { get; set; }
    public string? DigitalSignature { get; set; }   // HMAC/RSA signature over canonical content
    public bool SentToPharmacy { get; set; }
    public DateTime? DispensedAt { get; set; }
    public Guid? DispensedByPharmacistId { get; set; }
    public bool IsExpired => DateTime.UtcNow > ValidUntil;
}
