using EventFoto.Data.DTOs;
using EventFoto.Data.Models;
using EventFoto.Data.Repositories;

namespace EventFoto.Core.Events;

public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;

    public EventService(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<ServiceResult<IList<Event>>> GetAllEventsByUser(int userId)
    {
        var events = await _eventRepository.GetAllEventsByUser(userId);
        return ServiceResult<IList<Event>>.Ok(events);
    }
    
    public async Task<ServiceResult<Event>> CreateEventAsync(CreateEventDto eventDto, int userId)
    {
        var eventData = new Event
        {
            Name = eventDto.Name,
            CreatedBy = userId,
            IsArchived = false
        };
        eventData = await _eventRepository.CreateAsync(eventData);
        return ServiceResult<Event>.Ok(eventData);
    }
}
