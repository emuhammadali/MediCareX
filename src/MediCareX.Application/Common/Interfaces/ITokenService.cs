using MediCareX.Application.Features.Auth.DTOs;
using MediCareX.Domain.Entities;

namespace MediCareX.Application.Common.Interfaces;

public interface ITokenService
{
    /// <summary>Issues a short-lived JWT plus a rotating refresh token (hash stored in DB).</summary>
    Task<TokenDto> CreateTokensAsync(ApplicationUser user, string? ipAddress, CancellationToken ct);

    /// <summary>Rotates the refresh token. Reuse of a revoked token revokes the whole chain.</summary>
    Task<(TokenDto Tokens, ApplicationUser User)> RefreshAsync(string refreshToken, string? ipAddress, CancellationToken ct);

    Task RevokeAsync(string refreshToken, string? ipAddress, CancellationToken ct);
    Task RevokeAllForUserAsync(Guid userId, CancellationToken ct);
}
