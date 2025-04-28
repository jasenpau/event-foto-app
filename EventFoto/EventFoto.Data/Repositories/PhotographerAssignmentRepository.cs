using EventFoto.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EventFoto.Data.Repositories;

public class PhotographerAssignmentRepository : IPhotographerAssignmentRepository
{
    private readonly EventFotoContext _context;
    private DbSet<PhotographerAssignment> Assignments => _context.PhotographerAssignments;

    public PhotographerAssignmentRepository(EventFotoContext context)
    {
        _context = context;
    }

    public Task<PhotographerAssignment> GetForEventAsync(int eventId, Guid userId)
    {
        return Assignments
            .Include(a => a.User)
            .Include(a => a.Gallery)
            .SingleOrDefaultAsync(a => a.Gallery.EventId == eventId && a.UserId == userId);
    }

    public async Task<List<PhotographerAssignment>> GetAssignmentsForEventAsync(int eventId)
    {
        return await Assignments
            .Include(a => a.User)
            .Include(a => a.Gallery)
            .Where(a => a.Gallery.EventId == eventId)
            .ToListAsync();
    }

    public async Task<bool> AssignPhotographerAsync(PhotographerAssignment assignment)
    {
        Assignments.Add(assignment);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveAssignmentAsync(PhotographerAssignment assignment)
    {
        if (assignment == null)
            return false;

        Assignments.Remove(assignment);
        await _context.SaveChangesAsync();
        return true;
    }
}
