using MediCareX.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MediCareX.Application.Common.Interfaces;

/// <summary>Abstraction over the EF Core context so handlers never depend on Infrastructure.</summary>
public interface IApplicationDbContext
{
    DbSet<Patient> Patients { get; }
    DbSet<Doctor> Doctors { get; }
    DbSet<AvailabilitySlot> AvailabilitySlots { get; }
    DbSet<Appointment> Appointments { get; }
    DbSet<VideoSession> VideoSessions { get; }
    DbSet<VideoSessionParticipant> VideoSessionParticipants { get; }
    DbSet<Prescription> Prescriptions { get; }
    DbSet<Medicine> Medicines { get; }
    DbSet<LabTest> LabTests { get; }
    DbSet<LabReport> LabReports { get; }
    DbSet<Payment> Payments { get; }
    DbSet<Wallet> Wallets { get; }
    DbSet<Transaction> Transactions { get; }
    DbSet<ChatMessage> ChatMessages { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<Review> Reviews { get; }
    DbSet<MedicalRecord> MedicalRecords { get; }
    DbSet<HealthMetric> HealthMetrics { get; }
    DbSet<Symptom> Symptoms { get; }
    DbSet<Disease> Diseases { get; }
    DbSet<DiseaseSymptom> DiseaseSymptoms { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
