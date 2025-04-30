using System.Globalization;
using EventFoto.Data.DatabaseProjections;
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

    public async Task<Event> CreateAsync(Event eventData, Gallery defaultGallery)
    {
        eventData.CreatedOn = DateTime.UtcNow;

        await using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                Events.Add(eventData);
                await _context.SaveChangesAsync();

                defaultGallery.EventId = eventData.Id;
                _context.Galleries.Add(defaultGallery);
                await _context.SaveChangesAsync();

                eventData.DefaultGalleryId = defaultGallery.Id;
                _context.Entry(eventData).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await Events
                    .Include(e => e.CreatedByUser)
                    .FirstOrDefaultAsync(e => e.Id == eventData.Id);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }

    public async Task<PagedData<string, EventListProjection>> SearchEventsAsync(EventSearchParams searchParams)
    {
        var idOffset = 0;
        var dateOffset = DateTime.MinValue.ToUniversalTime();
        if (!string.IsNullOrWhiteSpace(searchParams.KeyOffset))
        {
            var offset = searchParams.KeyOffset.Split('|');
            dateOffset = DateTime.Parse(offset[0], CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
            idOffset = int.Parse(offset[1]);
        }

        var nameQuery = string.IsNullOrWhiteSpace(searchParams.Query) ? null : searchParams.Query;
        var fromDate = searchParams.FromDate;
        var toDate = searchParams.ToDate;
        var showArchived = searchParams.ShowArchived ?? false;
        // Add one more element to check for next page
        var pageSize = searchParams.PageSize + 1;

        var queryResult = await _context.Database
            .SqlQuery<EventListProjection>($"SELECT * FROM event_list_search({idOffset}, {dateOffset}, {nameQuery}, {fromDate}, {toDate}, {showArchived}, {pageSize})")
            .ToListAsync();

        var hasNextPage = queryResult.Count > searchParams.PageSize;
        // Remove last element to constrain to the queried page size.
        if (hasNextPage) queryResult.RemoveAt(queryResult.Count - 1);

        return new PagedData<string, EventListProjection>
        {
            Data = queryResult,
            PageSize = searchParams.PageSize,
            KeyOffset = searchParams.KeyOffset,
            HasNextPage = hasNextPage
        };
    }

    public async Task<Event> UpdateAsync(Event eventData)
    {
        _context.Entry(eventData).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return await GetByIdAsync(eventData.Id);
    }

    public Task<List<Event>> GetAllEventsInDatesAsync(DateTime startDate, DateTime endDate)
    {
        return Events.Where(e =>
                (e.StartDate >= startDate || (e.EndDate.HasValue && e.EndDate.Value >= startDate)) &&
                (e.StartDate <= endDate || (e.EndDate.HasValue && e.EndDate.Value <= endDate)))
            .ToListAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var event_ = await Events.FindAsync(id);
        if (event_ == null) return false;

        Events.Remove(event_);
        await _context.SaveChangesAsync();
        return true;
    }
}
