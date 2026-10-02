using MediCareX.Domain.Common;
using MediCareX.Domain.Enums;

namespace MediCareX.Domain.Entities;

public class Transaction : BaseEntity
{
    public Guid WalletId { get; set; }
    public Wallet Wallet { get; set; } = null!;
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public TransactionType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Reference { get; set; }
}
