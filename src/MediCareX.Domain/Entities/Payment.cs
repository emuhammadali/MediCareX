using MediCareX.Domain.Common;
using MediCareX.Domain.Enums;

namespace MediCareX.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "PKR";
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public PaymentPurpose Purpose { get; set; }
    /// <summary>Provider transaction / payment-intent id.</summary>
    public string? TransactionId { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal DoctorShare { get; set; }
    public decimal RefundedAmount { get; set; }
    public string? FailureReason { get; set; }
    public string? InvoiceUrl { get; set; }
    public string? ProviderPayloadJson { get; set; }
    public string? IdempotencyKey { get; set; }
    public DateTime? PaidAt { get; set; }
}
