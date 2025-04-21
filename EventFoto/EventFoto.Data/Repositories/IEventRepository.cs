using EventFoto.Data.DTOs;
using EventFoto.Data.Models;

namespace EventFoto.Data.Repositories;

public interface IEventRepository
{
    public Task<Event> GetByIdAsync(int id);
    public Task<Event> GetByIdWithPhotographersAsync(int id);
    public Task<IList<User>> GetEventPhotographersAsync(int eventId);
    public Task<Event> CreateAsync(Event eventData, Gallery defaultGallery);
    public Task<Event> SaveEventAsync(Event eventData);
    public Task<PagedData<string, Event>> SearchEventsAsync(EventSearchParams searchParams);
}
