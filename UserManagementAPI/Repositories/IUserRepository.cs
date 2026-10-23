using UserManagementAPI.Models;

namespace UserManagementAPI.Repositories;

public enum UpdateOutcome { Success, NotFound, DuplicateEmail }

public interface IUserRepository
{
    IReadOnlyList<User> GetAll();
    User? GetById(Guid id);
    User? Create(string name, string email, string department);
    UpdateOutcome Update(Guid id, string name, string email, string department, out User? user);
    bool Delete(Guid id);
}
