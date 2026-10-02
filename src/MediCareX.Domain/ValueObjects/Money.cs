namespace MediCareX.Domain.ValueObjects;

/// <summary>Immutable monetary amount. Arithmetic requires matching currency.</summary>
public sealed record Money(decimal Amount, string Currency = "PKR")
{
    public static Money Zero(string currency = "PKR") => new(0m, currency);
    public Money Add(Money o) { Same(o); return this with { Amount = Amount + o.Amount }; }
    public Money Subtract(Money o) { Same(o); return this with { Amount = Amount - o.Amount }; }
    public Money Percent(decimal pct) => this with { Amount = Math.Round(Amount * pct / 100m, 2, MidpointRounding.AwayFromZero) };
    private void Same(Money o) { if (!string.Equals(Currency, o.Currency, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Currency mismatch."); }
}
