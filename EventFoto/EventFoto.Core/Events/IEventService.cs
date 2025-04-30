using EventFoto.Data.DatabaseProjections;
using EventFoto.Data.DTOs;
using EventFoto.Data.Models;

namespace EventFoto.Core.Events;

public interface IEventService
{
    public Task<ServiceResult<Event>> GetById(int id);
    public Task<ServiceResult<Event>> CreateEventAsync(CreateEditEventDto eventDto, Guid userId);
    public Task<ServiceResult<PagedData<string, EventListProjection>>> SearchEventsAsync(EventSearchParams searchParams);
    public Task<ServiceResult<Event>> UpdateEventAsync(int id, CreateEditEventDto eventDto);
    public Task<ServiceResult<string>> ArchiveEventAsync(int eventId);
    public Task<ServiceResult<bool>> DeleteEventAsync(int id);
}
