namespace MediCareX.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default);
    Task SendEmailConfirmationAsync(string to, string name, Guid userId, string token, CancellationToken ct = default);
    Task SendPasswordResetAsync(string to, string name, string token, CancellationToken ct = default);
}
