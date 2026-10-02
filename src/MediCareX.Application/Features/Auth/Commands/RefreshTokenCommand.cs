using FluentValidation;
using MediatR;
using MediCareX.Application.Common.Interfaces;
using MediCareX.Application.Features.Auth.DTOs;
using MediCareX.Domain.Interfaces;

namespace MediCareX.Application.Features.Auth.Commands;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponseDto>;
public sealed record LogoutCommand(string RefreshToken) : IRequest;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator() => RuleFor(x => x.RefreshToken).NotEmpty();
}

public sealed class RefreshTokenCommandHandler(ITokenService tokens, IIdentityService identity, ICurrentUserService current)
    : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(RefreshTokenCommand r, CancellationToken ct)
    {
        var (issued, user) = await tokens.RefreshAsync(r.RefreshToken, current.IpAddress, ct);
        var roles = await identity.GetRolesAsync(user, ct);
        return new AuthResponseDto(UserDto.From(user, roles), issued, false);
    }
}

public sealed class LogoutCommandHandler(ITokenService tokens, ICurrentUserService current) : IRequestHandler<LogoutCommand>
{
    public Task Handle(LogoutCommand r, CancellationToken ct) => tokens.RevokeAsync(r.RefreshToken, current.IpAddress, ct);
}
