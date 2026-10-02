using MediatR;

namespace MediCareX.Domain.Common;

/// <summary>Marker for domain events dispatched through MediatR after SaveChanges.</summary>
public interface IDomainEvent : INotification { }
