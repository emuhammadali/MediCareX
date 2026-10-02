using MediCareX.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace MediCareX.Infrastructure.Services;

public sealed class EmailSettings
{
    public const string SectionName = "Email";
    /// <summary>"SendGrid" or "Log" (development: writes the email to the log instead of sending).</summary>
    public string Provider { get; set; } = "Log";
    public string? ApiKey { get; set; }
    public string FromEmail { get; set; } = "no-reply@medicarex.local";
    public string FromName { get; set; } = "MediCareX";
    public string ClientBaseUrl { get; set; } = "http://localhost:3000";
}

public sealed class EmailService(IOptions<EmailSettings> options, ILogger<EmailService> logger) : IEmailService
{
    private readonly EmailSettings _s = options.Value;

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        if (!_s.Provider.Equals("SendGrid", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(_s.ApiKey))
        {
            // DEV ONLY: bodies contain one-time tokens. Never use Provider=Log in production.
            logger.LogWarning("[EMAIL-LOG] To={To} Subject={Subject}\n{Body}", to, subject, htmlBody);
            return;
        }
        var client = new SendGridClient(_s.ApiKey);
        var msg = MailHelper.CreateSingleEmail(new EmailAddress(_s.FromEmail, _s.FromName), new EmailAddress(to), subject, null, htmlBody);
        var response = await client.SendEmailAsync(msg, ct);
        if (!response.IsSuccessStatusCode)
            logger.LogError("SendGrid rejected email with status {Status}", (int)response.StatusCode);
    }

    public Task SendEmailConfirmationAsync(string to, string name, Guid userId, string token, CancellationToken ct = default)
    {
        var link = $"{_s.ClientBaseUrl.TrimEnd('/')}/confirm-email?userId={userId}&token={Uri.EscapeDataString(token)}";
        return SendAsync(to, "Confirm your MediCareX account",
            $"<p>Hi {System.Net.WebUtility.HtmlEncode(name)},</p><p>Please <a href=\"{link}\">confirm your email</a> to activate your account.</p><p>{link}</p>", ct);
    }

    public Task SendPasswordResetAsync(string to, string name, string token, CancellationToken ct = default)
    {
        var link = $"{_s.ClientBaseUrl.TrimEnd('/')}/reset-password?email={Uri.EscapeDataString(to)}&token={Uri.EscapeDataString(token)}";
        return SendAsync(to, "Reset your MediCareX password",
            $"<p>Hi {System.Net.WebUtility.HtmlEncode(name)},</p><p><a href=\"{link}\">Reset your password</a>. If you did not request this, ignore this email.</p><p>{link}</p>", ct);
    }
}
