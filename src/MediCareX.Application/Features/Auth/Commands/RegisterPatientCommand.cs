using FluentValidation;
using MediatR;
using MediCareX.Application.Common.Interfaces;
using MediCareX.Application.Common.Validation;
using MediCareX.Application.Features.Auth.DTOs;
using MediCareX.Domain.Entities;
using MediCareX.Domain.Enums;
using MediCareX.Domain.ValueObjects;

namespace MediCareX.Application.Features.Auth.Commands;

public sealed record RegisterPatientCommand(
    string FirstName, string LastName, string Email, string Password, string ConfirmPassword,
    string? PhoneNumber, DateTime DateOfBirth, Gender Gender, string? Cnic, BloodGroup BloodGroup,
    string? Allergies, string? ChronicConditions, string? EmergencyContactName, string? EmergencyContactPhone)
    : IRequest<RegisteredUserDto>;

public sealed class RegisterPatientCommandValidator : AbstractValidator<RegisterPatientCommand>
{
    public RegisterPatientCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).StrongPassword();
        RuleFor(x => x.ConfirmPassword).Equal(x => x.Password).WithMessage("Passwords do not match.");
        RuleFor(x => x.PhoneNumber).ValidPhone();
        RuleFor(x => x.Cnic).ValidCnic();
        RuleFor(x => x.DateOfBirth).LessThan(DateTime.UtcNow).WithMessage("Date of birth must be in the past.")
                                   .GreaterThan(DateTime.UtcNow.AddYears(-130));
        RuleFor(x => x.EmergencyContactPhone).ValidPhone();
    }
}

public sealed class RegisterPatientCommandHandler(
    IIdentityService identity, IApplicationDbContext db, IEmailService email)
    : IRequestHandler<RegisterPatientCommand, RegisteredUserDto>
{
    public async Task<RegisteredUserDto> Handle(RegisterPatientCommand r, CancellationToken ct)
    {
        var user = new ApplicationUser
        {
            UserName = r.Email, Email = r.Email, FirstName = r.FirstName.Trim(), LastName = r.LastName.Trim(),
            PhoneNumber = PhoneNumber.TryCreate(r.PhoneNumber, out var p) ? p!.Value : null,
            Gender = r.Gender, DateOfBirth = r.DateOfBirth
        };
        var userId = await identity.CreateUserAsync(user, r.Password, nameof(UserRole.Patient), ct);

        var wallet = new Wallet { UserId = userId };
        var patient = new Patient
        {
            UserId = userId, WalletId = wallet.Id, DateOfBirth = r.DateOfBirth, Gender = r.Gender,
            BloodGroup = r.BloodGroup, Allergies = r.Allergies, ChronicConditions = r.ChronicConditions,
            Cnic = CNIC.TryCreate(r.Cnic, out var c) ? c!.Value : null,
            EmergencyContactName = r.EmergencyContactName,
            EmergencyContactPhone = PhoneNumber.TryCreate(r.EmergencyContactPhone, out var ep) ? ep!.Value : null
        };
        db.Wallets.Add(wallet);
        db.Patients.Add(patient);
        await db.SaveChangesAsync(ct);

        var token = await identity.GenerateEmailConfirmationTokenAsync(user, ct);
        await email.SendEmailConfirmationAsync(r.Email, user.FullName, userId, token, ct);
        return new RegisteredUserDto(userId, r.Email, true);
    }
}
