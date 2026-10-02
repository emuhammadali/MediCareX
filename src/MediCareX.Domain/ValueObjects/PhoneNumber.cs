using System.Text.RegularExpressions;

namespace MediCareX.Domain.ValueObjects;

/// <summary>E.164 phone number (e.g. +923001234567).</summary>
public sealed partial record PhoneNumber
{
    public string Value { get; }
    private PhoneNumber(string v) => Value = v;

    public static bool TryCreate(string? input, out PhoneNumber? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;
        var n = input.Replace(" ", "").Replace("-", "");
        if (n.StartsWith("03") && n.Length == 11) n = "+92" + n[1..]; // PK local format
        if (!E164().IsMatch(n)) return false;
        result = new PhoneNumber(n);
        return true;
    }
    public static PhoneNumber Create(string input) =>
        TryCreate(input, out var r) ? r! : throw new ArgumentException("Invalid phone number.", nameof(input));
    public override string ToString() => Value;

    [GeneratedRegex(@"^\+[1-9]\d{7,14}$")] private static partial Regex E164();
}
