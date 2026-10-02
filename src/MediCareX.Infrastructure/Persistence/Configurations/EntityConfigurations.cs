using MediCareX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediCareX.Infrastructure.Persistence.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> b)
    {
        b.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        b.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        b.Property(x => x.ProfilePictureUrl).HasMaxLength(500);
        b.Property(x => x.FcmToken).HasMaxLength(500);
        b.Property(x => x.ExternalProvider).HasMaxLength(50);
        b.Property(x => x.ExternalProviderId).HasMaxLength(200);
        b.Property(x => x.CreatedBy).HasMaxLength(50);
        b.Property(x => x.UpdatedBy).HasMaxLength(50);
        b.HasIndex(x => new { x.ExternalProvider, x.ExternalProviderId });
    }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
        b.Property(x => x.ReplacedByTokenHash).HasMaxLength(128);
        b.HasIndex(x => x.TokenHash).IsUnique();
        b.HasIndex(x => x.UserId);
        b.HasOne(x => x.User).WithMany(u => u.RefreshTokens).HasForeignKey(x => x.UserId);
    }
}

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> b)
    {
        b.OwnsOne(x => x.Address);
        b.Property(x => x.Cnic).HasMaxLength(13);
        b.HasIndex(x => x.Cnic).IsUnique().HasFilter("[Cnic] IS NOT NULL AND [IsDeleted] = 0");
        b.HasOne(x => x.User).WithOne(u => u.Patient).HasForeignKey<Patient>(x => x.UserId);
        b.HasIndex(x => x.UserId).IsUnique();
        b.HasOne(x => x.Wallet).WithMany().HasForeignKey(x => x.WalletId);
        b.HasOne(x => x.GuardianPatient).WithMany(x => x.Dependents).HasForeignKey(x => x.GuardianPatientId);
    }
}

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> b)
    {
        b.Property(x => x.Specialization).HasMaxLength(150).IsRequired();
        b.Property(x => x.Qualifications).HasMaxLength(500);
        b.Property(x => x.LicenseNumber).HasMaxLength(50).IsRequired();
        b.Property(x => x.Currency).HasMaxLength(3);
        b.HasIndex(x => x.LicenseNumber).IsUnique().HasFilter("[IsDeleted] = 0");
        b.HasIndex(x => x.Specialization);
        b.HasOne(x => x.User).WithOne(u => u.Doctor).HasForeignKey<Doctor>(x => x.UserId);
        b.HasIndex(x => x.UserId).IsUnique();
    }
}

public class AvailabilitySlotConfiguration : IEntityTypeConfiguration<AvailabilitySlot>
{
    public void Configure(EntityTypeBuilder<AvailabilitySlot> b)
    {
        b.HasOne(x => x.Doctor).WithMany(d => d.AvailabilitySlots).HasForeignKey(x => x.DoctorId);
        b.HasIndex(x => new { x.DoctorId, x.DayOfWeek });
    }
}

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> b)
    {
        b.Property(x => x.Symptoms).HasMaxLength(2000);
        b.Property(x => x.Notes).HasMaxLength(2000);
        b.Property(x => x.RejectionReason).HasMaxLength(500);
        b.HasOne(x => x.Patient).WithMany(p => p.Appointments).HasForeignKey(x => x.PatientId);
        b.HasOne(x => x.Doctor).WithMany(d => d.Appointments).HasForeignKey(x => x.DoctorId);
        b.HasOne(x => x.Payment).WithMany().HasForeignKey(x => x.PaymentId);
        b.Ignore(x => x.VideoSessionId); // relationship is owned by VideoSession.AppointmentId
        b.HasIndex(x => new { x.DoctorId, x.ScheduledAt });
        b.HasIndex(x => new { x.PatientId, x.ScheduledAt });
        b.HasIndex(x => x.Status);
    }
}

public class VideoSessionConfiguration : IEntityTypeConfiguration<VideoSession>
{
    public void Configure(EntityTypeBuilder<VideoSession> b)
    {
        b.Property(x => x.RoomId).HasMaxLength(64).IsRequired();
        b.HasIndex(x => x.RoomId).IsUnique();
        b.HasOne(x => x.Appointment).WithOne(a => a.VideoSession).HasForeignKey<VideoSession>(x => x.AppointmentId);
        b.HasMany(x => x.Participants).WithOne(p => p.VideoSession).HasForeignKey(p => p.VideoSessionId);
    }
}

public class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
{
    public void Configure(EntityTypeBuilder<Prescription> b)
    {
        b.Property(x => x.Diagnosis).HasMaxLength(1000).IsRequired();
        b.Property(x => x.VerificationCode).HasMaxLength(64).IsRequired();
        b.HasIndex(x => x.VerificationCode).IsUnique();
        b.OwnsMany(x => x.Medicines, o => o.ToJson());   // JSON column (EF Core 8)
        b.HasOne(x => x.Appointment).WithOne(a => a.Prescription).HasForeignKey<Prescription>(x => x.AppointmentId);
        b.HasOne(x => x.Doctor).WithMany().HasForeignKey(x => x.DoctorId);
        b.HasOne(x => x.Patient).WithMany(p => p.Prescriptions).HasForeignKey(x => x.PatientId);
    }
}

public class MedicineConfiguration : IEntityTypeConfiguration<Medicine>
{
    public void Configure(EntityTypeBuilder<Medicine> b)
    {
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.GenericName).HasMaxLength(200);
        b.HasIndex(x => x.Name);
        b.HasIndex(x => x.GenericName);
    }
}

public class LabTestConfiguration : IEntityTypeConfiguration<LabTest>
{
    public void Configure(EntityTypeBuilder<LabTest> b) => b.Property(x => x.Name).HasMaxLength(200).IsRequired();
}

public class LabReportConfiguration : IEntityTypeConfiguration<LabReport>
{
    public void Configure(EntityTypeBuilder<LabReport> b)
    {
        b.HasOne(x => x.Patient).WithMany(p => p.LabReports).HasForeignKey(x => x.PatientId);
        b.HasOne(x => x.Test).WithMany().HasForeignKey(x => x.TestId);
        b.HasOne(x => x.OrderedByDoctor).WithMany().HasForeignKey(x => x.OrderedByDoctorId);
        b.HasIndex(x => new { x.PatientId, x.TestId });
    }
}

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> b)
    {
        b.Property(x => x.Currency).HasMaxLength(3);
        b.Property(x => x.TransactionId).HasMaxLength(200);
        b.Property(x => x.IdempotencyKey).HasMaxLength(100);
        b.HasIndex(x => x.UserId);
        b.HasIndex(x => x.TransactionId);
        b.HasIndex(x => x.IdempotencyKey).IsUnique().HasFilter("[IdempotencyKey] IS NOT NULL");
    }
}

public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> b)
    {
        b.Property(x => x.Currency).HasMaxLength(3);
        b.Property(x => x.RowVersion).IsRowVersion();
        b.HasIndex(x => x.UserId).IsUnique().HasFilter("[IsDeleted] = 0");
        b.HasMany(x => x.Transactions).WithOne(t => t.Wallet).HasForeignKey(t => t.WalletId);
    }
}

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> b)
    {
        b.Property(x => x.Message).HasMaxLength(4000).IsRequired();
        b.HasIndex(x => new { x.SenderId, x.ReceiverId, x.SentAt });
        b.HasIndex(x => x.AppointmentId);
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> b)
    {
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Property(x => x.Message).HasMaxLength(2000).IsRequired();
        b.HasIndex(x => new { x.UserId, x.IsRead });
    }
}

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> b)
    {
        b.ToTable(t => t.HasCheckConstraint("CK_Review_Rating", "[Rating] BETWEEN 1 AND 5"));
        b.HasOne(x => x.Appointment).WithOne(a => a.Review).HasForeignKey<Review>(x => x.AppointmentId);
        b.HasOne(x => x.Doctor).WithMany(d => d.Reviews).HasForeignKey(x => x.DoctorId);
        b.HasOne(x => x.Patient).WithMany().HasForeignKey(x => x.PatientId);
    }
}

public class MedicalRecordConfiguration : IEntityTypeConfiguration<MedicalRecord>
{
    public void Configure(EntityTypeBuilder<MedicalRecord> b)
    {
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Property(x => x.ContentHash).HasMaxLength(64);
        b.Property(x => x.BlockchainTxHash).HasMaxLength(100);
        b.HasOne(x => x.Patient).WithMany(p => p.MedicalRecords).HasForeignKey(x => x.PatientId);
        b.HasIndex(x => new { x.PatientId, x.RecordType });
    }
}

public class HealthMetricConfiguration : IEntityTypeConfiguration<HealthMetric>
{
    public void Configure(EntityTypeBuilder<HealthMetric> b)
    {
        b.Property(x => x.Unit).HasMaxLength(20);
        b.HasOne(x => x.Patient).WithMany(p => p.HealthMetrics).HasForeignKey(x => x.PatientId);
        b.HasIndex(x => new { x.PatientId, x.Type, x.RecordedAt });
    }
}

public class SymptomConfiguration : IEntityTypeConfiguration<Symptom>
{
    public void Configure(EntityTypeBuilder<Symptom> b)
    {
        b.Property(x => x.Name).HasMaxLength(150).IsRequired();
        b.HasIndex(x => x.Name).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public class DiseaseConfiguration : IEntityTypeConfiguration<Disease>
{
    public void Configure(EntityTypeBuilder<Disease> b)
    {
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.HasIndex(x => x.Name).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public class DiseaseSymptomConfiguration : IEntityTypeConfiguration<DiseaseSymptom>
{
    public void Configure(EntityTypeBuilder<DiseaseSymptom> b)
    {
        b.HasKey(x => new { x.DiseaseId, x.SymptomId });
        b.HasOne(x => x.Disease).WithMany(d => d.DiseaseSymptoms).HasForeignKey(x => x.DiseaseId);
        b.HasOne(x => x.Symptom).WithMany(s => s.DiseaseSymptoms).HasForeignKey(x => x.SymptomId);
    }
}

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.Property(x => x.Action).HasMaxLength(50).IsRequired();
        b.Property(x => x.EntityName).HasMaxLength(100).IsRequired();
        b.Property(x => x.EntityId).HasMaxLength(64);
        b.Property(x => x.IpAddress).HasMaxLength(64);
        b.HasIndex(x => x.Timestamp);
        b.HasIndex(x => new { x.EntityName, x.EntityId });
        b.HasIndex(x => x.UserId);
    }
}
