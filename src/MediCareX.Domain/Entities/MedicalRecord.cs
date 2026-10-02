using MediCareX.Domain.Common;
using MediCareX.Domain.Enums;

namespace MediCareX.Domain.Entities;

public class MedicalRecord : BaseEntity
{
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public RecordType RecordType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string DataJson { get; set; } = "{}";
    public string? FileUrl { get; set; }
    /// <summary>SHA-256 of canonical record content (what is anchored on-chain).</summary>
    public string? ContentHash { get; set; }
    public string? BlockchainTxHash { get; set; }
    public bool Verified { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public Guid? CreatedByDoctorId { get; set; }
}
