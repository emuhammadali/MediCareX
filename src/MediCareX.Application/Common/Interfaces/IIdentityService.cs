using MediCareX.Domain.Entities;

namespace MediCareX.Application.Common.Interfaces;

public enum SignInOutcome { Success, InvalidCredentials, LockedOut, EmailNotConfirmed, RequiresTwoFactor, Inactive }

public sealed record CredentialCheckResult(SignInOutcome Outcome, ApplicationUser? User);

public sealed record TwoFactorSetup(string SharedKey, string AuthenticatorUri);

/// <summary>Wraps ASP.NET Core Identity so the Application layer stays framework-agnostic.</summary>
public interface IIdentityService
{
    /// <summary>Creates the user and assigns the role. Throws ValidationException on Identity errors.</summary>
    Task<Guid> CreateUserAsync(ApplicationUser user, string password, string role, CancellationToken ct);
    Task<ApplicationUser?> FindByIdAsync(Guid id, CancellationToken ct);
    Task<ApplicationUser?> FindByEmailAsync(string email, CancellationToken ct);
    Task<IReadOnlyList<string>> GetRolesAsync(ApplicationUser user, CancellationToken ct);

    /// <summary>Validates password, lockout (increments failure count) and confirmation state.</summary>
    Task<CredentialCheckResult> CheckCredentialsAsync(string email, string password, CancellationToken ct);

    Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user, CancellationToken ct);
    Task<bool> ConfirmEmailAsync(Guid userId, string token, CancellationToken ct);
    Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user, CancellationToken ct);
    Task ResetPasswordAsync(ApplicationUser user, string token, string newPassword, CancellationToken ct);
    Task ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword, CancellationToken ct);

    Task<TwoFactorSetup> BeginTwoFactorSetupAsync(ApplicationUser user, CancellationToken ct);
    /// <summary>Verifies a TOTP code. When <paramref name="enable"/> is true a valid code also turns 2FA on.</summary>
    Task<bool> VerifyTwoFactorCodeAsync(ApplicationUser user, string code, bool enable, CancellationToken ct);
    Task SetLastLoginAsync(ApplicationUser user, CancellationToken ct);
}
