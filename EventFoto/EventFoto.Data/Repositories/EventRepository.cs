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

    public Task<Event> GetByIdWithPhotographersAsync(int id)
    {
        return Events
            .Include(e => e.CreatedByUser)
            .Include(e => e.Photographers)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IList<Event>> GetAllEventsAsync()
    {
        return await Events.ToListAsync();
    }

    public async Task<IList<User>> GetEventPhotographersAsync(int eventId)
    {
        var selectedEvent = await Events.Include(e => e.Photographers)
            .FirstOrDefaultAsync(e => e.Id == eventId);
        return selectedEvent?.Photographers;
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

    public async Task<Event> UpdateEventAsync(Event eventData)
    {
        _context.Entry(eventData).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return eventData;
    }
}
