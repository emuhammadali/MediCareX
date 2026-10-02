using FluentValidation;
using MediatR;
using MediCareX.Application.Common.Exceptions;
using MediCareX.Application.Common.Interfaces;
using MediCareX.Application.Features.Auth.DTOs;
using MediCareX.Domain.Enums;
using MediCareX.Domain.Interfaces;

namespace MediCareX.Application.Features.Auth.Queries;

public sealed record GetCurrentUserQuery : IRequest<UserDto>;
public sealed record GetUserProfileQuery(Guid UserId) : IRequest<UserDto>;

public sealed class GetUserProfileQueryValidator : AbstractValidator<GetUserProfileQuery>
{ public GetUserProfileQueryValidator() => RuleFor(x => x.UserId).NotEmpty(); }

public sealed class GetCurrentUserQueryHandler(IIdentityService identity, ICurrentUserService current)
    : IRequestHandler<GetCurrentUserQuery, UserDto>
{
    public async Task<UserDto> Handle(GetCurrentUserQuery q, CancellationToken ct)
    {
        var id = current.UserId ?? throw new UnauthorizedException();
        var user = await identity.FindByIdAsync(id, ct) ?? throw new NotFoundException("User", id);
        return UserDto.From(user, await identity.GetRolesAsync(user, ct));
    }
}

public sealed class GetUserProfileQueryHandler(IIdentityService identity, ICurrentUserService current)
    : IRequestHandler<GetUserProfileQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserProfileQuery q, CancellationToken ct)
    {
        if (current.UserId != q.UserId && !current.IsInRole(nameof(UserRole.Admin)))
            throw new ForbiddenAccessException();
        var user = await identity.FindByIdAsync(q.UserId, ct) ?? throw new NotFoundException("User", q.UserId);
        return UserDto.From(user, await identity.GetRolesAsync(user, ct));
    }
}
