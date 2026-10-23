using UserManagementAPI.Models;

namespace UserManagementAPI.Repositories;

/// <summary>
/// Thread-safe in-memory sample repository. Data is lost when the app restarts.
/// Keeps an email index so uniqueness checks do not scan every user.
/// </summary>
public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly object _gate = new();
    private readonly Dictionary<Guid, User> _users = new();
    private readonly Dictionary<string, Guid> _emails = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<User> GetAll()
    {
        lock (_gate)
            return _users.Values.OrderBy(u => u.CreatedAtUtc).ThenBy(u => u.Id).ToArray();
    }

    public User? GetById(Guid id)
    {
        lock (_gate)
            return _users.GetValueOrDefault(id);
    }

    public User? Create(string name, string email, string department)
    {
        name = name.Trim();
        email = email.Trim();
        department = department.Trim();
        lock (_gate)
        {
            if (_emails.ContainsKey(email)) return null;
            var now = DateTimeOffset.UtcNow;
            var user = new User(Guid.NewGuid(), name, email, department, now, now);
            _users.Add(user.Id, user);
            _emails.Add(email, user.Id);
            return user;
        }
    }

    public UpdateOutcome Update(Guid id, string name, string email, string department, out User? user)
    {
        name = name.Trim();
        email = email.Trim();
        department = department.Trim();
        lock (_gate)
        {
            user = null;
            if (!_users.TryGetValue(id, out var existing))
                return UpdateOutcome.NotFound;

            if (_emails.TryGetValue(email, out var ownerId) && ownerId != id)
                return UpdateOutcome.DuplicateEmail;

            if (!string.Equals(existing.Email, email, StringComparison.OrdinalIgnoreCase))
            {
                _emails.Remove(existing.Email);
                _emails[email] = id;
            }
            user = existing with
            {
                Name = name,
                Email = email,
                Department = department,
                UpdatedAtUtc = DateTimeOffset.UtcNow
            };
            _users[id] = user;
            return UpdateOutcome.Success;
        }
    }

    public bool Delete(Guid id)
    {
        lock (_gate)
        {
            if (!_users.Remove(id, out var user)) return false;
            _emails.Remove(user.Email);
            return true;
        }
    }
}
