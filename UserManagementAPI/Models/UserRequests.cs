namespace UserManagementAPI.Models;

// Nullable incoming properties let our validator return consistent 400 responses
// when clients omit properties or send explicit nulls.
public sealed class CreateUserRequest
{
    public string? Name { get; init; }
    public string? Email { get; init; }
    public string? Department { get; init; }
}

public sealed class UpdateUserRequest
{
    public string? Name { get; init; }
    public string? Email { get; init; }
    public string? Department { get; init; }
}
