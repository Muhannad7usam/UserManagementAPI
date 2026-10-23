using System.Security.Cryptography;
using System.Text;

namespace UserManagementAPI.Middleware;

/// <summary>
/// Optional demo-level API-key protection for the user endpoints.
/// Set ApiSecurity__ApiKey in the environment; do not commit real keys.
/// </summary>
public sealed class ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var configuredKey = configuration["ApiSecurity:ApiKey"];
        if (string.IsNullOrWhiteSpace(configuredKey) ||
            !context.Request.Path.StartsWithSegments("/api/users"))
        {
            await next(context);
            return;
        }

        var suppliedKey = context.Request.Headers["X-API-Key"].ToString();
        var expectedBytes = Encoding.UTF8.GetBytes(configuredKey);
        var suppliedBytes = Encoding.UTF8.GetBytes(suppliedKey);
        if (!CryptographicOperations.FixedTimeEquals(expectedBytes, suppliedBytes))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await Results.Json(new { error = "Missing or invalid API key." }, statusCode: 401)
                .ExecuteAsync(context);
            return;
        }

        await next(context);
    }
}
