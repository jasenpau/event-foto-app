using EventFoto.Data.DTOs;
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

    public Task<User> GetByInvitationKeyAsync(string invitationKey) =>
        Users.SingleOrDefaultAsync(u => u.InvitationKey == invitationKey);

    public Task<User> GetUserByEmailAsync(string email) =>
        Users.SingleOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

    public async Task<User> CreateUserAsync(User user)
    {
        Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateUserAsync(User user)
    {
        _context.Entry(user).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<PagedData<string, User>> SearchUsersAsync(UserSearchParams searchParams)
    {
        IQueryable<User> query = Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchParams.KeyOffset))
        {
            query = query.Where(u => u.Name.CompareTo(searchParams.KeyOffset) > 0);
        }

        if (!string.IsNullOrWhiteSpace(searchParams.Query))
        {
            query = query.Where(u =>
                EF.Functions.ILike(u.Name, $"%{searchParams.Query}%") ||
                EF.Functions.ILike(u.Email, $"%{searchParams.Query}%"));
        }

        if (searchParams.ExcludeEventId.HasValue && searchParams.ExcludeEventId.Value > 0)
        {
            query = query.Where(u => u.GroupAssignment != null); // Filter only photographers
            query.Include(u => u.Assignments).ThenInclude(a => a.Gallery);
            query = query.Where(u => u.Assignments.All(e => e.Gallery.EventId != searchParams.ExcludeEventId.Value));
        }

        query = query.OrderBy(u => u.Name).ThenBy(u => u.Email);
        // Add +1 to the query to check, if we have the next page.
        query = query.Take(searchParams.PageSize + 1);

        var queryResult = await query.ToListAsync();
        var hasNextPage = queryResult.Count > searchParams.PageSize;
        // Remove last element to constrain to the queried page size.
        if (hasNextPage) queryResult.RemoveAt(queryResult.Count - 1);

        return new PagedData<string, User>
        {
            Data = queryResult,
            PageSize = searchParams.PageSize,
            KeyOffset = searchParams.KeyOffset,
            HasNextPage = hasNextPage
        };
    }
}
