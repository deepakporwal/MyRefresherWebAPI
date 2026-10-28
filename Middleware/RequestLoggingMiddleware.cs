using System.Diagnostics;

namespace MyRefresherWebAPI.Middleware;

public sealed class RequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var traceId = context.TraceIdentifier;

        logger.LogInformation(
            "HTTP request started. Method {Method} Path {Path} TraceId {TraceId}",
            context.Request.Method,
            context.Request.Path,
            traceId);

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();

            logger.LogInformation(
                "HTTP request completed. Method {Method} Path {Path} StatusCode {StatusCode} " +
                "ElapsedMilliseconds {ElapsedMilliseconds} TraceId {TraceId} User {User}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                traceId,
                context.User.Identity?.Name ?? "anonymous");
        }
    }
}
