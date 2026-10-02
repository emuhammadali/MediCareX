using MediCareX.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace MediCareX.API.Middleware;

/// <summary>Maps application exceptions to RFC 7807 ProblemDetails. Internal details never leak to clients.</summary>
public sealed class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try { await next(ctx); }
        catch (OperationCanceledException) when (ctx.RequestAborted.IsCancellationRequested) { ctx.Response.StatusCode = 499; }
        catch (Exception ex) { await WriteAsync(ctx, ex); }
    }

    private async Task WriteAsync(HttpContext ctx, Exception ex)
    {
        ProblemDetails problem;
        switch (ex)
        {
            case ValidationException v:
                problem = new ValidationProblemDetails(v.Errors) { Status = 400, Title = "Validation failed" }; break;
            case NotFoundException: problem = Make(404, "Not found", ex.Message); break;
            case UnauthorizedException: problem = Make(401, "Unauthorized", ex.Message); break;
            case ForbiddenAccessException: problem = Make(403, "Forbidden", ex.Message); break;
            case ConflictException: problem = Make(409, "Conflict", ex.Message); break;
            case BusinessRuleException: problem = Make(422, "Business rule violated", ex.Message); break;
            default:
                logger.LogError(ex, "Unhandled exception on {Method} {Path}", ctx.Request.Method, ctx.Request.Path);
                problem = Make(500, "Server error", env.IsDevelopment() ? ex.Message : "An unexpected error occurred.");
                break;
        }
        problem.Instance = ctx.Request.Path;
        problem.Extensions["traceId"] = ctx.TraceIdentifier;
        ctx.Response.StatusCode = problem.Status ?? 500;
        ctx.Response.ContentType = "application/problem+json";
        await ctx.Response.WriteAsJsonAsync(problem, problem.GetType());
    }

    private static ProblemDetails Make(int status, string title, string detail) => new() { Status = status, Title = title, Detail = detail };
}
