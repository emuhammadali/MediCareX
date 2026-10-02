using MediatR;
using MediCareX.Application.Features.Auth.Commands;
using MediCareX.Application.Features.Auth.DTOs;
using MediCareX.Application.Features.Auth.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace MediCareX.API.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
[EnableRateLimiting("auth")]
public sealed class AuthController(ISender mediator) : ControllerBase
{
    /// <summary>Register a patient account (email confirmation required before login).</summary>
    [HttpPost("register/patient")]
    [ProducesResponseType(typeof(RegisteredUserDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> RegisterPatient(RegisterPatientCommand cmd, CancellationToken ct)
        => StatusCode(StatusCodes.Status201Created, await mediator.Send(cmd, ct));

    /// <summary>Register a doctor account. Stays pending until an admin approves.</summary>
    [HttpPost("register/doctor")]
    [ProducesResponseType(typeof(RegisteredUserDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> RegisterDoctor(RegisterDoctorCommand cmd, CancellationToken ct)
        => StatusCode(StatusCodes.Status201Created, await mediator.Send(cmd, ct));

    /// <summary>Login. If requiresTwoFactor=true, call /verify-2fa next.</summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginCommand cmd, CancellationToken ct) => Ok(await mediator.Send(cmd, ct));

    [HttpPost("verify-2fa")]
    public async Task<ActionResult<AuthResponseDto>> Verify2FA(Verify2FACommand cmd, CancellationToken ct) => Ok(await mediator.Send(cmd, ct));

    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthResponseDto>> Refresh(RefreshTokenCommand cmd, CancellationToken ct) => Ok(await mediator.Send(cmd, ct));

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutCommand cmd, CancellationToken ct) { await mediator.Send(cmd, ct); return NoContent(); }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] Guid userId, [FromQuery] string token, CancellationToken ct)
    { await mediator.Send(new ConfirmEmailCommand(userId, token), ct); return NoContent(); }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> Forgot(ForgotPasswordCommand cmd, CancellationToken ct) { await mediator.Send(cmd, ct); return Accepted(); }

    [HttpPost("reset-password")]
    public async Task<IActionResult> Reset(ResetPasswordCommand cmd, CancellationToken ct) { await mediator.Send(cmd, ct); return NoContent(); }

    [Authorize, HttpPost("change-password")]
    public async Task<IActionResult> Change(ChangePasswordCommand cmd, CancellationToken ct) { await mediator.Send(cmd, ct); return NoContent(); }

    /// <summary>Step 1 of 2FA: returns secret + otpauth URI (render as QR).</summary>
    [Authorize, HttpPost("2fa/enable")]
    public async Task<ActionResult<TwoFactorSetupDto>> Enable2FA(CancellationToken ct) => Ok(await mediator.Send(new Enable2FACommand(), ct));

    /// <summary>Step 2 of 2FA: confirm with a code from Google Authenticator.</summary>
    [Authorize, HttpPost("2fa/confirm")]
    public async Task<IActionResult> Confirm2FA(Confirm2FASetupCommand cmd, CancellationToken ct) { await mediator.Send(cmd, ct); return NoContent(); }

    [Authorize, HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me(CancellationToken ct) => Ok(await mediator.Send(new GetCurrentUserQuery(), ct));

    [Authorize, HttpGet("users/{id:guid}")]
    public async Task<ActionResult<UserDto>> Profile(Guid id, CancellationToken ct) => Ok(await mediator.Send(new GetUserProfileQuery(id), ct));
}
