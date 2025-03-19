using EventFoto.Data.DTOs;
using EventFoto.Data.Models;

namespace EventFoto.Core.Events;

public interface IEventService
{
    public Task<ServiceResult<IList<Event>>> GetAllEventsByUser(Guid userId);
    public Task<ServiceResult<Event>> CreateEventAsync(CreateEventDto eventDto, Guid userId);
}
