using EventFoto.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EventFoto.Data.Repositories;

public class EventRepository : IEventRepository 
{
    private readonly EventFotoContext _context;
    private DbSet<Event> Events => _context.Events;

    public EventRepository(EventFotoContext context)
    {
        _context = context;
    }

    public Task<Event> GetByIdAsync(int id)
    {
        return Events
            .Include(e => e.CreatedByUser)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IList<Event>> GetAllEventsByUserAsync(Guid userId)
    {
        return await Events.Where(e => e.CreatedBy == userId).ToListAsync();
    }
    
    public async Task<Event> CreateAsync(Event eventData)
    {
        eventData.CreatedOn = DateTime.UtcNow;
        Events.Add(eventData);
        await _context.SaveChangesAsync();
        return await Events
            .Include(e => e.CreatedByUser)
            .FirstOrDefaultAsync(e => e.Id == eventData.Id);
    }
}
