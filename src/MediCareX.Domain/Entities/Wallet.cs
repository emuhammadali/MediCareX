using MediCareX.Domain.Common;
using MediCareX.Domain.Enums;

namespace MediCareX.Domain.Entities;

public class Wallet : BaseEntity
{
    public Guid UserId { get; set; }
    public decimal Balance { get; private set; }
    public string Currency { get; set; } = "PKR";
    public bool IsActive { get; set; } = true;
    /// <summary>Optimistic concurrency token — prevents double-spend.</summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public Transaction Credit(decimal amount, string description, string? reference = null)
    {
        EnsureActive(); EnsurePositive(amount);
        Balance += amount;
        return Record(amount, TransactionType.Credit, description, reference);
    }

    public Transaction Debit(decimal amount, string description, string? reference = null)
    {
        EnsureActive(); EnsurePositive(amount);
        if (Balance < amount) throw new InvalidOperationException("Insufficient wallet balance.");
        Balance -= amount;
        return Record(amount, TransactionType.Debit, description, reference);
    }

    private Transaction Record(decimal amount, TransactionType type, string description, string? reference)
    {
        var t = new Transaction { WalletId = Id, Amount = amount, Type = type, Description = description, Reference = reference, BalanceAfter = Balance };
        Transactions.Add(t);
        return t;
    }
    private void EnsureActive() { if (!IsActive) throw new InvalidOperationException("Wallet is inactive."); }
    private static void EnsurePositive(decimal a) { if (a <= 0) throw new ArgumentOutOfRangeException(nameof(a), "Amount must be positive."); }
}
