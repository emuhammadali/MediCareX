using MediCareX.Domain.Common;
using MediCareX.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace MediCareX.Domain.Entities;

/// <summary>Application account. Identity supplies email, phone, 2FA, lockout and password hash.</summary>
public class ApplicationUser : IdentityUser<Guid>, IAuditableEntity, ISoftDelete
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public Gender Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? FcmToken { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public string? ExternalProvider { get; set; }   // Google / Facebook
    public string? ExternalProviderId { get; set; }

    // Notification preferences
    public bool NotifyByEmail { get; set; } = true;
    public bool NotifyBySms { get; set; } = true;
    public bool NotifyByPush { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Patient? Patient { get; set; }
    public Doctor? Doctor { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() { }
    public ApplicationRole(string name) : base(name) { }
}
