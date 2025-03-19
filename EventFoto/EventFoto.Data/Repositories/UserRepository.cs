using EventFoto.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EventFoto.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly EventFotoContext _context;
    private DbSet<User> Users => _context.Users;

    public UserRepository(EventFotoContext context)
    {
        _context = context;
    }
    
    public Task<User> GetUserByIdAsync(Guid userId) =>
        Users.SingleOrDefaultAsync(u => u.Id == userId);

    public async Task<User> CreateUserAsync(User user)
    {
        Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }
}
