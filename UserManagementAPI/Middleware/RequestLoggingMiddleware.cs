using System.Diagnostics;

namespace UserManagementAPI.Middleware;

public sealed class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var timer = Stopwatch.StartNew();
        try
        {
            await next(context);
        }
        finally
        {
            timer.Stop();
            // Never log request bodies, email addresses, query strings or API keys.
            logger.LogInformation(
                "{Method} {Path} -> {StatusCode} in {ElapsedMs} ms (TraceId {TraceId})",
                context.Request.Method,
                context.Request.Path.Value,
                context.Response.StatusCode,
                timer.ElapsedMilliseconds,
                context.TraceIdentifier);
        }
    }
}
