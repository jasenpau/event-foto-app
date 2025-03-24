using EventFoto.Data.DTOs;
using EventFoto.Data.Models;

namespace EventFoto.Core.Events;

public interface IEventService
{
    public Task<ServiceResult<Event>> GetById(int id);
    public Task<ServiceResult<IList<Event>>> GetAllEventsByUserAsync(Guid userId);
    public Task<ServiceResult<Event>> CreateEventAsync(CreateEventDto eventDto, Guid userId);
}
