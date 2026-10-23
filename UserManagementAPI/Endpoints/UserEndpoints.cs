using UserManagementAPI.Models;
using UserManagementAPI.Repositories;
using UserManagementAPI.Validation;

namespace UserManagementAPI.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var users = app.MapGroup("/api/users").WithTags("Users");

        users.MapGet("", (IUserRepository repository) =>
            Results.Ok(repository.GetAll()))
            .WithName("GetUsers");

        users.MapGet("/{id:guid}", (Guid id, IUserRepository repository) =>
        {
            var user = repository.GetById(id);
            return user is null ? NotFound(id) : Results.Ok(user);
        }).WithName("GetUserById");

        users.MapPost("", (CreateUserRequest request, IUserRepository repository) =>
        {
            var errors = UserValidator.Validate(request.Name, request.Email, request.Department);
            if (errors.Count > 0) return Results.ValidationProblem(errors);

            var user = repository.Create(request.Name!, request.Email!, request.Department!);
            if (user is null)
                return Results.Conflict(new { error = "A user with this email already exists." });

            return Results.Created($"/api/users/{user.Id}", user);
        }).WithName("CreateUser");

        users.MapPut("/{id:guid}", (Guid id, UpdateUserRequest request, IUserRepository repository) =>
        {
            var errors = UserValidator.Validate(request.Name, request.Email, request.Department);
            if (errors.Count > 0) return Results.ValidationProblem(errors);

            var outcome = repository.Update(id, request.Name!, request.Email!, request.Department!, out var user);
            return outcome switch
            {
                UpdateOutcome.Success => Results.Ok(user),
                UpdateOutcome.NotFound => NotFound(id),
                UpdateOutcome.DuplicateEmail => Results.Conflict(new { error = "A user with this email already exists." }),
                _ => Results.Problem(statusCode: StatusCodes.Status500InternalServerError)
            };
        }).WithName("UpdateUser");

        users.MapDelete("/{id:guid}", (Guid id, IUserRepository repository) =>
            repository.Delete(id) ? Results.NoContent() : NotFound(id))
            .WithName("DeleteUser");

        return app;
    }

    private static IResult NotFound(Guid id) =>
        Results.NotFound(new { error = $"User with ID '{id}' was not found." });
}
