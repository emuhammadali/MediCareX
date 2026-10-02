using MediCareX.Domain.Common;

namespace MediCareX.Domain.Entities;

/// <summary>Chat message metadata. Message history may additionally be mirrored to MongoDB.</summary>
public class ChatMessage : BaseEntity
{
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    public Guid? AppointmentId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
}
