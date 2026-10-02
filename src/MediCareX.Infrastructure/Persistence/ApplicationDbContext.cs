using System.Linq.Expressions;
using System.Text.Json;
using MediCareX.Application.Common.Interfaces;
using MediCareX.Domain.Common;
using MediCareX.Domain.Entities;
using MediCareX.Domain.Interfaces;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MediCareX.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUserService currentUser)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options), IApplicationDbContext
{
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<AvailabilitySlot> AvailabilitySlots => Set<AvailabilitySlot>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<VideoSession> VideoSessions => Set<VideoSession>();
    public DbSet<VideoSessionParticipant> VideoSessionParticipants => Set<VideoSessionParticipant>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<Medicine> Medicines => Set<Medicine>();
    public DbSet<LabTest> LabTests => Set<LabTest>();
    public DbSet<LabReport> LabReports => Set<LabReport>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
    public DbSet<HealthMetric> HealthMetrics => Set<HealthMetric>();
    public DbSet<Symptom> Symptoms => Set<Symptom>();
    public DbSet<Disease> Diseases => Set<Disease>();
    public DbSet<DiseaseSymptom> DiseaseSymptoms => Set<DiseaseSymptom>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // SQL Server rejects multiple cascade paths; soft-delete makes cascades pointless anyway.
        foreach (var fk in builder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            fk.DeleteBehavior = DeleteBehavior.Restrict;

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global soft-delete filter
        foreach (var et in builder.Model.GetEntityTypes().Where(e => typeof(ISoftDelete).IsAssignableFrom(e.ClrType)))
        {
            var p = Expression.Parameter(et.ClrType, "e");
            var body = Expression.Equal(Expression.Property(p, nameof(ISoftDelete.IsDeleted)), Expression.Constant(false));
            et.SetQueryFilter(Expression.Lambda(body, p));
        }
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var userId = currentUser.UserId?.ToString();
        var ip = currentUser.IpAddress;
        var audits = new List<AuditLog>();
        var ignoredProps = new HashSet<string> { "UpdatedAt", "UpdatedBy", "PasswordHash", "SecurityStamp", "ConcurrencyStamp" };

        foreach (var entry in ChangeTracker.Entries().ToList())
        {
            if (entry.State is EntityState.Detached or EntityState.Unchanged) continue;

            var isDelete = false;
            if (entry.Entity is ISoftDelete sd && entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                sd.IsDeleted = true; sd.DeletedAt = now; isDelete = true;
            }
            if (entry.Entity is IAuditableEntity a)
            {
                if (entry.State == EntityState.Added) { a.CreatedAt = now; a.CreatedBy = userId; }
                else if (entry.State == EntityState.Modified) { a.UpdatedAt = now; a.UpdatedBy = userId; }
            }

            // Audit trail: entity + id + changed property NAMES only (values may be PHI/PII).
            if (entry.Entity is RefreshToken or AuditLog) continue;
            if (entry.Entity is not (BaseEntity or ApplicationUser)) continue;
            var changed = entry.State == EntityState.Modified
                ? entry.Properties.Where(x => x.IsModified && !ignoredProps.Contains(x.Metadata.Name)).Select(x => x.Metadata.Name).ToArray()
                : Array.Empty<string>();
            audits.Add(new AuditLog
            {
                UserId = currentUser.UserId,
                Action = isDelete ? "Deleted" : entry.State == EntityState.Added ? "Created" : "Updated",
                EntityName = entry.Entity.GetType().Name,
                EntityId = entry.Property("Id").CurrentValue?.ToString(),
                NewValues = changed.Length > 0 ? JsonSerializer.Serialize(changed) : null,
                IpAddress = ip, Timestamp = now
            });
        }

        if (audits.Count > 0) AuditLogs.AddRange(audits);
        return await base.SaveChangesAsync(cancellationToken);
    }
}
