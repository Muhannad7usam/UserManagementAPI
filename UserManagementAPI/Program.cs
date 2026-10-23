using UserManagementAPI.Endpoints;
using UserManagementAPI.Middleware;
using UserManagementAPI.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Singleton is intentional: this demonstration uses one thread-safe in-memory store.
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();

var app = builder.Build();

// Order matters: logging captures the final status, including handled failures.
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<ApiKeyMiddleware>();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .WithName("HealthCheck");

app.MapUserEndpoints();
app.Run();

// Useful when adding integration tests with WebApplicationFactory later.
public partial class Program { }
