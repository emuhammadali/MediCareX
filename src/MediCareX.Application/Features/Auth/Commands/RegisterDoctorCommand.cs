using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MediCareX.Application.Common.Interfaces;
using MediCareX.Application.Common.Validation;
using MediCareX.Application.Features.Auth.DTOs;
using MediCareX.Domain.Entities;
using MediCareX.Domain.Enums;
using MediCareX.Domain.ValueObjects;

namespace MediCareX.Application.Features.Auth.Commands;

public sealed record RegisterDoctorCommand(
    string FirstName, string LastName, string Email, string Password, string ConfirmPassword,
    string? PhoneNumber, Gender Gender, string Specialization, string Qualifications,
    int ExperienceYears, string LicenseNumber, decimal ConsultationFee, string? Bio)
    : IRequest<RegisteredUserDto>;

public sealed class RegisterDoctorCommandValidator : AbstractValidator<RegisterDoctorCommand>
{
    public RegisterDoctorCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).StrongPassword();
        RuleFor(x => x.ConfirmPassword).Equal(x => x.Password).WithMessage("Passwords do not match.");
        RuleFor(x => x.PhoneNumber).ValidPhone();
        RuleFor(x => x.Specialization).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Qualifications).NotEmpty().MaximumLength(500);
        RuleFor(x => x.ExperienceYears).InclusiveBetween(0, 70);
        RuleFor(x => x.LicenseNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ConsultationFee).GreaterThanOrEqualTo(0);
    }
}

public sealed class RegisterDoctorCommandHandler(IIdentityService identity, IApplicationDbContext db, IEmailService email)
    : IRequestHandler<RegisterDoctorCommand, RegisteredUserDto>
{
    public async Task<RegisteredUserDto> Handle(RegisterDoctorCommand r, CancellationToken ct)
    {
        if (await db.Doctors.AnyAsync(d => d.LicenseNumber == r.LicenseNumber, ct))
            throw new Common.Exceptions.ConflictException("A doctor with this license number is already registered.");

        var user = new ApplicationUser
        {
            UserName = r.Email, Email = r.Email, FirstName = r.FirstName.Trim(), LastName = r.LastName.Trim(),
            PhoneNumber = PhoneNumber.TryCreate(r.PhoneNumber, out var p) ? p!.Value : null, Gender = r.Gender
        };
        var userId = await identity.CreateUserAsync(user, r.Password, nameof(UserRole.Doctor), ct);

        // Doctors stay unapproved until an Admin runs ApproveDoctorCommand.
        db.Doctors.Add(new Doctor
        {
            UserId = userId, Specialization = r.Specialization, Qualifications = r.Qualifications,
            ExperienceYears = r.ExperienceYears, LicenseNumber = r.LicenseNumber,
            ConsultationFee = r.ConsultationFee, Bio = r.Bio, IsApproved = false
        });
        db.Wallets.Add(new Wallet { UserId = userId });
        await db.SaveChangesAsync(ct);

        var token = await identity.GenerateEmailConfirmationTokenAsync(user, ct);
        await email.SendEmailConfirmationAsync(r.Email, user.FullName, userId, token, ct);
        return new RegisteredUserDto(userId, r.Email, true);
    }
}
