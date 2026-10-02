using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MediCareX.Application.Common.Exceptions;
using MediCareX.Application.Common.Interfaces;
using MediCareX.Application.Features.Auth.DTOs;
using MediCareX.Domain.Entities;
using MediCareX.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MediCareX.Infrastructure.Identity;

public sealed class TokenService(UserManager<ApplicationUser> users, ApplicationDbContext db, IOptions<JwtSettings> options) : ITokenService
{
    private readonly JwtSettings _s = options.Value;

    public async Task<TokenDto> CreateTokensAsync(ApplicationUser user, string? ipAddress, CancellationToken ct)
    {
        var (raw, entity) = NewRefreshToken(user.Id, ipAddress);
        db.RefreshTokens.Add(entity);
        await db.SaveChangesAsync(ct);
        return await BuildAsync(user, raw, entity.ExpiresAt);
    }

    public async Task<(TokenDto Tokens, ApplicationUser User)> RefreshAsync(string refreshToken, string? ipAddress, CancellationToken ct)
    {
        var hash = Hash(refreshToken);
        var stored = await db.RefreshTokens.Include(t => t.User).FirstOrDefaultAsync(t => t.TokenHash == hash, ct)
                     ?? throw new UnauthorizedException("Invalid refresh token.");

        if (stored.RevokedAt is not null)
        {
            // A revoked token was presented again => possible theft. Kill every session for this user.
            await RevokeAllForUserAsync(stored.UserId, ct);
            throw new UnauthorizedException("Refresh token reuse detected. Please sign in again.");
        }
        if (stored.IsExpired) throw new UnauthorizedException("Refresh token expired.");
        if (!stored.User.IsActive) throw new ForbiddenAccessException("This account has been suspended.");

        var (raw, next) = NewRefreshToken(stored.UserId, ipAddress);
        stored.RevokedAt = DateTime.UtcNow;
        stored.RevokedByIp = ipAddress;
        stored.ReplacedByTokenHash = next.TokenHash;
        db.RefreshTokens.Add(next);
        await db.SaveChangesAsync(ct);

        return (await BuildAsync(stored.User, raw, next.ExpiresAt), stored.User);
    }

    public async Task RevokeAsync(string refreshToken, string? ipAddress, CancellationToken ct)
    {
        var hash = Hash(refreshToken);
        var stored = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);
        if (stored is null || stored.RevokedAt is not null) return;
        stored.RevokedAt = DateTime.UtcNow;
        stored.RevokedByIp = ipAddress;
        await db.SaveChangesAsync(ct);
    }

    public Task RevokeAllForUserAsync(Guid userId, CancellationToken ct) =>
        db.RefreshTokens.Where(t => t.UserId == userId && t.RevokedAt == null)
          .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, (DateTime?)DateTime.UtcNow), ct);

    private async Task<TokenDto> BuildAsync(ApplicationUser user, string rawRefresh, DateTime refreshExpires)
    {
        var roles = await users.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_s.Secret));
        var expires = DateTime.UtcNow.AddMinutes(_s.AccessTokenMinutes);
        var jwt = new JwtSecurityToken(_s.Issuer, _s.Audience, claims, DateTime.UtcNow, expires,
            new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new TokenDto(new JwtSecurityTokenHandler().WriteToken(jwt), rawRefresh, expires, refreshExpires);
    }

    private (string Raw, RefreshToken Entity) NewRefreshToken(Guid userId, string? ip)
    {
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        return (raw, new RefreshToken
        {
            UserId = userId, TokenHash = Hash(raw), CreatedByIp = ip,
            ExpiresAt = DateTime.UtcNow.AddDays(_s.RefreshTokenDays)
        });
    }

    private static string Hash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
