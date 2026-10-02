namespace MediCareX.Domain.ValueObjects;

/// <summary>Postal address with optional geo-coordinates (used for heatmaps and SOS).</summary>
public sealed record Address(string Line1, string? Line2, string City, string? State, string Country, string? PostalCode, double? Latitude = null, double? Longitude = null)
{
    public static Address Empty => new(string.Empty, null, string.Empty, null, "Pakistan", null);
}
