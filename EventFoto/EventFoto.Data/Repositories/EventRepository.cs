using System.Globalization;
using EventFoto.Data.DTOs;
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

    public async Task<PagedData<string, Event>> SearchEventsAsync(EventSearchParams searchParams)
    {
        IQueryable<Event> query = Events.Include(e => e.CreatedByUser);

        if (!string.IsNullOrWhiteSpace(searchParams.KeyOffset))
        {
            var offset = searchParams.KeyOffset.Split('|');
            var offsetDate = DateTime.Parse(offset[0], CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
            var offsetId = int.Parse(offset[1]);
            query = query.Where(e => e.StartDate >= offsetDate && e.Id > offsetId);
        }

        if (!string.IsNullOrWhiteSpace(searchParams.Query))
        {
            query = query.Where(e => EF.Functions.ILike(e.Name, $"%{searchParams.Query}%"));
        }

        if (searchParams.FromDate.HasValue)
        {
            query = query.Where(e =>
                e.StartDate >= searchParams.FromDate.Value ||
                (e.EndDate.HasValue && e.EndDate.Value >= searchParams.FromDate.Value)
            );
        }

        if (searchParams.ToDate.HasValue)
        {
            query = query.Where(e =>
                e.StartDate <= searchParams.ToDate.Value ||
                (e.EndDate.HasValue && e.EndDate.Value <= searchParams.ToDate.Value)
            );
        }

        if (searchParams.ShowArchived is null or false)
        {
            query = query.Where(e => e.IsArchived == false);
        }

        query = query.OrderBy(e => e.StartDate).ThenBy(e => e.Id);
        // Add +1 to the query to check, if we have the next page.
        query = query.Take(searchParams.PageSize + 1);

        var queryResult = await query.ToListAsync();
        var hasNextPage = queryResult.Count > searchParams.PageSize;
        // Remove last element to constrain to the queried page size.
        if (hasNextPage) queryResult.RemoveAt(queryResult.Count - 1);

        return new PagedData<string, Event>
        {
            Data = queryResult,

            PageSize = searchParams.PageSize,
            KeyOffset = searchParams.KeyOffset,
            HasNextPage = hasNextPage
        };
    }
}
