namespace MediCareX.Infrastructure.Identity;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";
    /// <summary>HMAC key, min 32 chars. Supply via user-secrets / env var (Jwt__Secret) - never commit.</summary>
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "MediCareX";
    public string Audience { get; set; } = "MediCareX.Clients";
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;
}
