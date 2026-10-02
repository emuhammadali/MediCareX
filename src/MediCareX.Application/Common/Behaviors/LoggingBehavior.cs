using MediatR;
using MediCareX.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace MediCareX.Application.Common.Behaviors;

/// <summary>Logs request name + user id only. Never logs the request body (passwords, medical data = PII).</summary>
public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger, ICurrentUserService user)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var name = typeof(TRequest).Name;
        logger.LogInformation("Handling {Request} by {UserId}", name, user.UserId);
        var response = await next();
        logger.LogInformation("Handled {Request}", name);
        return response;
    }
}
