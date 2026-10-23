namespace UserManagementAPI.Models;

public sealed record User(
    Guid Id,
    string Name,
    string Email,
    string Department,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
