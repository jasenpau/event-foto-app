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

    public async Task<IList<Event>> GetAllEventsByUser(Guid userId)
    {
        return await Events.Where(e => e.CreatedBy == userId).ToListAsync();
    }
    
    public async Task<Event> CreateAsync(Event eventData)
    {
        eventData.CreatedOn = DateTime.UtcNow;
        Events.Add(eventData);
        await _context.SaveChangesAsync();
        return eventData;
    }
}
