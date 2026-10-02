using FluentValidation;
using MediCareX.Domain.ValueObjects;

namespace MediCareX.Application.Common.Validation;

public static class ValidationExtensions
{
    /// <summary>Min 8 chars, upper, lower, digit, symbol (matches Identity options configured in Infrastructure).</summary>
    public static IRuleBuilderOptions<T, string> StrongPassword<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().MinimumLength(8).MaximumLength(100)
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain a digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain a symbol.");

    public static IRuleBuilderOptions<T, string?> ValidCnic<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(v => string.IsNullOrWhiteSpace(v) || CNIC.TryCreate(v, out _)).WithMessage("CNIC must be 13 digits (e.g. 12345-1234567-1).");

    public static IRuleBuilderOptions<T, string?> ValidPhone<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(v => string.IsNullOrWhiteSpace(v) || PhoneNumber.TryCreate(v, out _)).WithMessage("Enter a valid phone number (e.g. +923001234567 or 03001234567).");
}
