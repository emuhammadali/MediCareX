using MediatR;
using MediCareX.Application.Common.Exceptions;
using Microsoft.Extensions.Logging;

namespace MediCareX.Application.Common.Behaviors;

public sealed class UnhandledExceptionBehavior<TRequest, TResponse>(ILogger<TRequest> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        try { return await next(); }
        catch (Exception ex) when (ex is not (ValidationException or NotFoundException or ForbiddenAccessException
                                       or UnauthorizedException or ConflictException or BusinessRuleException or OperationCanceledException))
        {
            logger.LogError(ex, "Unhandled exception for request {Request}", typeof(TRequest).Name);
            throw;
        }
    }
}
