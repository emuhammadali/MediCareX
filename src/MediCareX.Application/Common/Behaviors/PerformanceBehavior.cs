using System.Diagnostics;
using MediatR;
using MediCareX.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace MediCareX.Application.Common.Behaviors;

public sealed class PerformanceBehavior<TRequest, TResponse>(ILogger<PerformanceBehavior<TRequest, TResponse>> logger, ICurrentUserService user)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private const int ThresholdMs = 500;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        var response = await next();
        sw.Stop();
        if (sw.ElapsedMilliseconds > ThresholdMs)
            logger.LogWarning("Long running request {Request} ({Elapsed} ms) by {UserId}", typeof(TRequest).Name, sw.ElapsedMilliseconds, user.UserId);
        return response;
    }
}
