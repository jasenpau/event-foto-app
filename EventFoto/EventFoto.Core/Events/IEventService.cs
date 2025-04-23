using EventFoto.Data.DatabaseProjections;
using EventFoto.Data.DTOs;
using EventFoto.Data.Models;

namespace EventFoto.Core.Events;

public interface IEventService
{
    public Task<ServiceResult<Event>> GetById(int id);
    public Task<ServiceResult<IList<EventPhotographerDto>>> GetEventPhotographersAsync(int eventId);
    public Task<ServiceResult<IList<EventPhotographerDto>>> AssignPhotographerAsync(int eventId, Guid userId);
    public Task<ServiceResult<IList<EventPhotographerDto>>> UnassignPhotographerAsync(int eventId, Guid userId);
    public Task<ServiceResult<Event>> CreateEventAsync(CreateEventDto eventDto, Guid userId);
    public Task<ServiceResult<PagedData<string, Event>>> SearchEventsAsync(EventSearchParams searchParams);
    public Task<ServiceResult<Gallery>> CreateGalleryAsync(int eventId, string name);
    public Task<ServiceResult<List<EventGalleryProjection>>> GetGalleriesAsync(int eventId);
}
