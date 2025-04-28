using EventFoto.Data.DatabaseProjections;
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
    public Task<PagedData<string, EventListProjection>> SearchEventsAsync(EventSearchParams searchParams);
    public Task<Event> UpdateAsync(Event eventData);
}
