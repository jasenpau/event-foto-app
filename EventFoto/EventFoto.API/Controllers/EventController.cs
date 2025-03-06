using EventFoto.API.Extensions;
using EventFoto.Core.Events;
using EventFoto.Data.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventFoto.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EventController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet]
    public async Task<ActionResult<EventDto[]>> GetAllEvents()
    {
        var userId = HttpContext.RequestUserId();
        var events = await _eventService.GetAllEventsByUser(userId);
        var eventDtos = events.Data.Select(EventDto.FromEvent).ToArray();
        return Ok(eventDtos);
    }
    
    [HttpPost]
    public async Task<ActionResult<EventDto>> CreateEvent([FromBody] CreateEventDto createEventDto)
    {
        var userId = HttpContext.RequestUserId();
        var createdEvent = await _eventService.CreateEventAsync(createEventDto, userId);
        return EventDto.FromEvent(createdEvent.Data);
    }
}
