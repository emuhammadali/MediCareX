using MediCareX.Domain.Entities;

namespace MediCareX.Application.Features.Auth.DTOs;

public sealed record TokenDto(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresAt, DateTime RefreshTokenExpiresAt);

public sealed record UserDto(
    Guid Id, string Email, string FullName, string? PhoneNumber, string? ProfilePictureUrl,
    IReadOnlyList<string> Roles, bool EmailConfirmed, bool TwoFactorEnabled)
{
    public static UserDto From(ApplicationUser u, IReadOnlyList<string> roles) =>
        new(u.Id, u.Email ?? string.Empty, u.FullName, u.PhoneNumber, u.ProfilePictureUrl, roles, u.EmailConfirmed, u.TwoFactorEnabled);
}

/// <summary>When RequiresTwoFactor is true, Tokens is null and the client must call verify-2fa.</summary>
public sealed record AuthResponseDto(UserDto? User, TokenDto? Tokens, bool RequiresTwoFactor);

public sealed record RegisteredUserDto(Guid UserId, string Email, bool EmailConfirmationRequired);

public sealed record TwoFactorSetupDto(string SharedKey, string AuthenticatorUri);
