using System.Text.RegularExpressions;

namespace MediCareX.Domain.ValueObjects;

/// <summary>Pakistani CNIC, stored as 13 digits; formatted as 12345-1234567-1.</summary>
public sealed partial record CNIC
{
    public string Value { get; }
    private CNIC(string v) => Value = v;

    public static bool TryCreate(string? input, out CNIC? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;
        var digits = input.Replace("-", "").Trim();
        if (!Digits13().IsMatch(digits)) return false;
        result = new CNIC(digits);
        return true;
    }
    public static CNIC Create(string input) =>
        TryCreate(input, out var r) ? r! : throw new ArgumentException("CNIC must be 13 digits.", nameof(input));
    public string Formatted => $"{Value[..5]}-{Value[5..12]}-{Value[12]}";
    public override string ToString() => Formatted;

    [GeneratedRegex(@"^\d{13}$")] private static partial Regex Digits13();
}
