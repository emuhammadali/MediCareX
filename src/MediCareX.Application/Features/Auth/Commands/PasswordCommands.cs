using FluentValidation;
using MediatR;
using MediCareX.Application.Common.Exceptions;
using MediCareX.Application.Common.Interfaces;
using MediCareX.Application.Common.Validation;
using MediCareX.Domain.Interfaces;

// Fix for ValidationException ambiguity
using ValidationException = MediCareX.Application.Common.Exceptions.ValidationException;

namespace MediCareX.Application.Features.Auth.Commands;

public sealed record ForgotPasswordCommand(string Email) : IRequest;
public sealed record ResetPasswordCommand(string Email, string Token, string NewPassword, string ConfirmPassword) : IRequest;
public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword, string ConfirmPassword) : IRequest;
public sealed record ConfirmEmailCommand(Guid UserId, string Token) : IRequest;

public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{ public ForgotPasswordCommandValidator() => RuleFor(x => x.Email).NotEmpty().EmailAddress(); }

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.NewPassword).StrongPassword();
        RuleFor(x => x.ConfirmPassword).Equal(x => x.NewPassword).WithMessage("Passwords do not match.");
    }
}

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).StrongPassword().NotEqual(x => x.CurrentPassword).WithMessage("New password must differ from the current one.");
        RuleFor(x => x.ConfirmPassword).Equal(x => x.NewPassword).WithMessage("Passwords do not match.");
    }
}

public sealed class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Token).NotEmpty();
    }
}

/// <summary>Always succeeds silently so attackers cannot enumerate registered emails.</summary>
public sealed class ForgotPasswordCommandHandler(IIdentityService identity, IEmailService email) : IRequestHandler<ForgotPasswordCommand>
{
    public async Task Handle(ForgotPasswordCommand r, CancellationToken ct)
    {
        var user = await identity.FindByEmailAsync(r.Email, ct);
        if (user is null || !user.EmailConfirmed) return;
        var token = await identity.GeneratePasswordResetTokenAsync(user, ct);
        await email.SendPasswordResetAsync(r.Email, user.FullName, token, ct);
    }
}

public sealed class ResetPasswordCommandHandler(IIdentityService identity, ITokenService tokens) : IRequestHandler<ResetPasswordCommand>
{
    public async Task Handle(ResetPasswordCommand r, CancellationToken ct)
    {
        var user = await identity.FindByEmailAsync(r.Email, ct) ?? throw new ValidationException(nameof(r.Token), "Invalid or expired reset token.");
        await identity.ResetPasswordAsync(user, r.Token, r.NewPassword, ct);
        await tokens.RevokeAllForUserAsync(user.Id, ct); // sign out everywhere
    }
}

public sealed class ChangePasswordCommandHandler(IIdentityService identity, ITokenService tokens, ICurrentUserService current)
    : IRequestHandler<ChangePasswordCommand>
{
    public async Task Handle(ChangePasswordCommand r, CancellationToken ct)
    {
        var id = current.UserId ?? throw new UnauthorizedException();
        var user = await identity.FindByIdAsync(id, ct) ?? throw new NotFoundException("User", id);
        await identity.ChangePasswordAsync(user, r.CurrentPassword, r.NewPassword, ct);
        await tokens.RevokeAllForUserAsync(id, ct);
    }
}

public sealed class ConfirmEmailCommandHandler(IIdentityService identity) : IRequestHandler<ConfirmEmailCommand>
{
    public async Task Handle(ConfirmEmailCommand r, CancellationToken ct)
    {
        if (!await identity.ConfirmEmailAsync(r.UserId, r.Token, ct))
            throw new ValidationException(nameof(r.Token), "Invalid or expired confirmation link.");
    }
}
