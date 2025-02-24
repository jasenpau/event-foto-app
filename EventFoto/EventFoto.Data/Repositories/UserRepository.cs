using EventFoto.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EventFoto.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DbSet<User> _users;

    public UserRepository(EventFotoContext context)
    {
        _users = context.Users;
    }
    
    public Task<User> GetUserByIdAsync(int userId) =>
        _users.SingleOrDefaultAsync(u => u.Id == userId);

    public Task<User> GetUserByEmailAsync(string email) =>
        _users.SingleOrDefaultAsync(u => u.Email == email);

    public Task<User> GetUserWithCredentialsAsync(string email)
    {
        var user = _users.Include(u => u.Credentials);
        return user.SingleOrDefaultAsync(u => u.Email == email);
    }
}
