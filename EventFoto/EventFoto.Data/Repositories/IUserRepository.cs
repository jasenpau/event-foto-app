using EventFoto.Data.Models;

namespace EventFoto.Data.Repositories;

public interface IUserRepository
{
    public Task<User> GetUserByIdAsync(Guid userId);
    public Task<User> CreateUserAsync(User user);
}
