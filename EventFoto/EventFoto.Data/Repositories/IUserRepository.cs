using EventFoto.Data.Models;

namespace EventFoto.Data.Repositories;

public interface IUserRepository
{
    public Task<User> GetUserByIdAsync(int userId);
    public Task<User> GetUserByEmailAsync(string email);
    public Task<User> GetUserWithCredentialsAsync(string email);
    public Task<User> CreateUserAsync(User user);
}
