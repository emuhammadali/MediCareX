using MediCareX.Domain.Common;

namespace MediCareX.Domain.Entities;

public class VideoSession : BaseEntity
{
    public Guid AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;
    public string RoomId { get; set; } = Guid.NewGuid().ToString("N");
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public bool RecordingConsentGiven { get; set; }
    public string? RecordingUrl { get; set; }
    public string? TranscriptSummary { get; set; }
    public ICollection<VideoSessionParticipant> Participants { get; set; } = new List<VideoSessionParticipant>();
    public TimeSpan? Duration => StartedAt is null || EndedAt is null ? null : EndedAt - StartedAt;
}

public class VideoSessionParticipant : BaseEntity
{
    public Guid VideoSessionId { get; set; }
    public VideoSession VideoSession { get; set; } = null!;
    public Guid UserId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LeftAt { get; set; }
}
