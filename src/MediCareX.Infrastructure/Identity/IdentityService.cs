using FluentValidation.Results;
using MediCareX.Application.Common.Exceptions;
using MediCareX.Application.Common.Interfaces;
using MediCareX.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace MediCareX.Infrastructure.Identity;

public sealed class IdentityService(UserManager<ApplicationUser> users) : IIdentityService
{
    private const string AppName = "MediCareX";

    public async Task<Guid> CreateUserAsync(ApplicationUser user, string password, string role, CancellationToken ct)
    {
        if (user.Id == Guid.Empty) user.Id = Guid.NewGuid();
        var created = await users.CreateAsync(user, password);
        if (!created.Succeeded) throw ToValidation(created);
        var inRole = await users.AddToRoleAsync(user, role);
        if (!inRole.Succeeded) throw ToValidation(inRole);
        return user.Id;
    }

    public Task<ApplicationUser?> FindByIdAsync(Guid id, CancellationToken ct) => users.FindByIdAsync(id.ToString());
    public Task<ApplicationUser?> FindByEmailAsync(string email, CancellationToken ct) => users.FindByEmailAsync(email);

    public async Task<IReadOnlyList<string>> GetRolesAsync(ApplicationUser user, CancellationToken ct)
        => (await users.GetRolesAsync(user)).ToList();

    public async Task<CredentialCheckResult> CheckCredentialsAsync(string email, string password, CancellationToken ct)
    {
        var user = await users.FindByEmailAsync(email);
        if (user is null) return new(SignInOutcome.InvalidCredentials, null);
        if (await users.IsLockedOutAsync(user)) return new(SignInOutcome.LockedOut, null);

        if (!await users.CheckPasswordAsync(user, password))
        {
            await users.AccessFailedAsync(user);
            return await users.IsLockedOutAsync(user)
                ? new(SignInOutcome.LockedOut, null)
                : new(SignInOutcome.InvalidCredentials, null);
        }

        await users.ResetAccessFailedCountAsync(user);
        if (!user.EmailConfirmed) return new(SignInOutcome.EmailNotConfirmed, null);
        if (!user.IsActive) return new(SignInOutcome.Inactive, null);
        return user.TwoFactorEnabled ? new(SignInOutcome.RequiresTwoFactor, user) : new(SignInOutcome.Success, user);
    }

    public Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user, CancellationToken ct) => users.GenerateEmailConfirmationTokenAsync(user);

    public async Task<bool> ConfirmEmailAsync(Guid userId, string token, CancellationToken ct)
    {
        var user = await users.FindByIdAsync(userId.ToString());
        return user is not null && (await users.ConfirmEmailAsync(user, token)).Succeeded;
    }

    public Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user, CancellationToken ct) => users.GeneratePasswordResetTokenAsync(user);

    public async Task ResetPasswordAsync(ApplicationUser user, string token, string newPassword, CancellationToken ct)
    {
        var r = await users.ResetPasswordAsync(user, token, newPassword);
        if (!r.Succeeded) throw ToValidation(r);
    }

    public async Task ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword, CancellationToken ct)
    {
        var r = await users.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!r.Succeeded) throw ToValidation(r);
    }

    public async Task<TwoFactorSetup> BeginTwoFactorSetupAsync(ApplicationUser user, CancellationToken ct)
    {
        if (user.TwoFactorEnabled) throw new ConflictException("Two-factor authentication is already enabled.");
        await users.ResetAuthenticatorKeyAsync(user);
        var key = await users.GetAuthenticatorKeyAsync(user) ?? throw new InvalidOperationException("Could not create authenticator key.");
        var label = Uri.EscapeDataString($"{AppName}:{user.Email}");
        var uri = $"otpauth://totp/{label}?secret={key}&issuer={Uri.EscapeDataString(AppName)}&digits=6";
        return new TwoFactorSetup(key, uri);
    }

    public async Task<bool> VerifyTwoFactorCodeAsync(ApplicationUser user, string code, bool enable, CancellationToken ct)
    {
        var clean = code.Replace(" ", "").Replace("-", "");
        var ok = await users.VerifyTwoFactorTokenAsync(user, users.Options.Tokens.AuthenticatorTokenProvider, clean);
        if (!ok)
        {
            if (!enable) await users.AccessFailedAsync(user); // brute-force protection for login step
            return false;
        }
        if (enable) await users.SetTwoFactorEnabledAsync(user, true);
        return true;
    }

    public async Task SetLastLoginAsync(ApplicationUser user, CancellationToken ct)
    {
        user.LastLoginAt = DateTime.UtcNow;
        await users.UpdateAsync(user);
    }

    private static ValidationException ToValidation(IdentityResult r) =>
        new(r.Errors.Select(e => new ValidationFailure(MapKey(e.Code), e.Description)));

    private static string MapKey(string code) =>
        code.Contains("Password", StringComparison.OrdinalIgnoreCase) ? "Password"
        : code.Contains("Email", StringComparison.OrdinalIgnoreCase) || code.Contains("UserName", StringComparison.OrdinalIgnoreCase) ? "Email"
        : code.Contains("Token", StringComparison.OrdinalIgnoreCase) ? "Token"
        : "Identity";
}
