using FluentValidation;
using MediatR;
using MediCareX.Application.Common.Exceptions;
using MediCareX.Application.Common.Interfaces;
using MediCareX.Application.Features.Auth.DTOs;
using MediCareX.Domain.Interfaces;

namespace MediCareX.Application.Features.Auth.Commands;

public sealed record LoginCommand(string Email, string Password) : IRequest<AuthResponseDto>;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public sealed class LoginCommandHandler(IIdentityService identity, ITokenService tokens, ICurrentUserService current)
    : IRequestHandler<LoginCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(LoginCommand r, CancellationToken ct)
    {
        var result = await identity.CheckCredentialsAsync(r.Email, r.Password, ct);
        switch (result.Outcome)
        {
            case SignInOutcome.InvalidCredentials: throw new UnauthorizedException("Invalid email or password.");
            case SignInOutcome.LockedOut: throw new ForbiddenAccessException("Account locked due to repeated failed attempts. Try again later.");
            case SignInOutcome.EmailNotConfirmed: throw new ForbiddenAccessException("Please confirm your email before signing in.");
            case SignInOutcome.Inactive: throw new ForbiddenAccessException("This account has been suspended.");
            case SignInOutcome.RequiresTwoFactor: return new AuthResponseDto(null, null, true);
        }

        var user = result.User!;
        var roles = await identity.GetRolesAsync(user, ct);
        var issued = await tokens.CreateTokensAsync(user, current.IpAddress, ct);
        await identity.SetLastLoginAsync(user, ct);
        return new AuthResponseDto(UserDto.From(user, roles), issued, false);
    }
}
