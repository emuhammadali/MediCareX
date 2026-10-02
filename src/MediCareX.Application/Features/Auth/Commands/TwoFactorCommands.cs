using FluentValidation;
using MediatR;
using MediCareX.Application.Common.Exceptions;
using MediCareX.Application.Common.Interfaces;
using MediCareX.Application.Features.Auth.DTOs;
using MediCareX.Domain.Interfaces;

// Fix for ValidationException ambiguity
using ValidationException = MediCareX.Application.Common.Exceptions.ValidationException;

namespace MediCareX.Application.Features.Auth.Commands;

/// <summary>Step 1: generates the TOTP secret + otpauth:// URI (render as QR in the client).</summary>
public sealed record Enable2FACommand : IRequest<TwoFactorSetupDto>;

/// <summary>Step 2: user types the 6-digit code from Google Authenticator to switch 2FA on.</summary>
public sealed record Confirm2FASetupCommand(string Code) : IRequest;

/// <summary>Login step 2 for users who already have 2FA enabled.</summary>
public sealed record Verify2FACommand(string Email, string Password, string Code) : IRequest<AuthResponseDto>;

public sealed class Confirm2FASetupCommandValidator : AbstractValidator<Confirm2FASetupCommand>
{
    public Confirm2FASetupCommandValidator() => RuleFor(x => x.Code).NotEmpty().Length(6).Matches("^[0-9]{6}$");
}

public sealed class Verify2FACommandValidator : AbstractValidator<Verify2FACommand>
{
    public Verify2FACommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().Length(6).Matches("^[0-9]{6}$");
    }
}

public sealed class Enable2FACommandHandler(IIdentityService identity, ICurrentUserService current)
    : IRequestHandler<Enable2FACommand, TwoFactorSetupDto>
{
    public async Task<TwoFactorSetupDto> Handle(Enable2FACommand r, CancellationToken ct)
    {
        var user = await identity.FindByIdAsync(current.UserId ?? throw new UnauthorizedException(), ct)
                   ?? throw new NotFoundException("User", current.UserId!);
        var setup = await identity.BeginTwoFactorSetupAsync(user, ct);
        return new TwoFactorSetupDto(setup.SharedKey, setup.AuthenticatorUri);
    }
}

public sealed class Confirm2FASetupCommandHandler(IIdentityService identity, ICurrentUserService current)
    : IRequestHandler<Confirm2FASetupCommand>
{
    public async Task Handle(Confirm2FASetupCommand r, CancellationToken ct)
    {
        var user = await identity.FindByIdAsync(current.UserId ?? throw new UnauthorizedException(), ct)
                   ?? throw new NotFoundException("User", current.UserId!);
        if (!await identity.VerifyTwoFactorCodeAsync(user, r.Code, enable: true, ct))
            throw new ValidationException(nameof(r.Code), "Invalid authenticator code.");
    }
}

public sealed class Verify2FACommandHandler(IIdentityService identity, ITokenService tokens, ICurrentUserService current)
    : IRequestHandler<Verify2FACommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(Verify2FACommand r, CancellationToken ct)
    {
        // Re-check the password so a leaked email alone can't be used to brute-force TOTP codes.
        var check = await identity.CheckCredentialsAsync(r.Email, r.Password, ct);
        if (check.Outcome is not (SignInOutcome.Success or SignInOutcome.RequiresTwoFactor) || check.User is null)
            throw new UnauthorizedException("Invalid credentials.");

        if (!await identity.VerifyTwoFactorCodeAsync(check.User, r.Code, enable: false, ct))
            throw new UnauthorizedException("Invalid authenticator code.");

        var roles = await identity.GetRolesAsync(check.User, ct);
        var issued = await tokens.CreateTokensAsync(check.User, current.IpAddress, ct);
        await identity.SetLastLoginAsync(check.User, ct);
        return new AuthResponseDto(UserDto.From(check.User, roles), issued, false);
    }
}
