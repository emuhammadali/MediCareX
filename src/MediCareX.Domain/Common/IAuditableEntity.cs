namespace MediCareX.Domain.Common;

/// <summary>Entities whose audit columns are stamped automatically by the DbContext.</summary>
public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
    string? CreatedBy { get; set; }
    string? UpdatedBy { get; set; }
}

/// <summary>Entities that are soft-deleted instead of physically removed.</summary>
public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
}
