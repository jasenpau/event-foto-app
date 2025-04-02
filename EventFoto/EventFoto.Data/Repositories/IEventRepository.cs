using EventFoto.Data.DTOs;
using EventFoto.Data.Models;

namespace EventFoto.Data.Repositories;

public interface IEventRepository
{
    public Task<Event> GetByIdAsync(int id);
    public Task<Event> GetByIdWithPhotographersAsync(int id);
    public Task<IList<Event>> GetAllEventsAsync();
    public Task<IList<User>> GetEventPhotographersAsync(int eventId);
    public Task<Event> CreateAsync(Event eventData);
    public Task<Event> UpdateEventAsync(Event eventData);
    public Task<PagedData<string, Event>> SearchEventsAsync(EventSearchParams searchParams);
}
