namespace UserManagementAPI.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled request exception. TraceId: {TraceId}", context.TraceIdentifier);
            if (context.Response.HasStarted) throw;

            context.Response.Clear();
            await Results.Problem(
                title: "Unexpected server error",
                detail: "An unexpected error occurred. Contact support with the trace ID.",
                statusCode: StatusCodes.Status500InternalServerError,
                extensions: new Dictionary<string, object?> { ["traceId"] = context.TraceIdentifier })
                .ExecuteAsync(context);
        }
    }
}
