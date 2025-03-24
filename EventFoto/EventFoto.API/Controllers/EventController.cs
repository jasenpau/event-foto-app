using EventFoto.API.Extensions;
using EventFoto.API.Filters;
using EventFoto.Core.Events;
using EventFoto.Data.DTOs;
using EventFoto.Data.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventFoto.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EventController : AppControllerBase
{
    private readonly IEventService _eventService;

    public EventController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet]
    public async Task<ActionResult<EventListDto[]>> GetAllEvents()
    {
        var userId = RequestUserId();
        var events = await _eventService.GetAllEventsByUserAsync(userId);
        var eventDtos = events.Data.Select(EventListDto.FromEvent).ToArray();
        return Ok(eventDtos);
    }
    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<EventListDto[]>> GetEventById(int id)
    {
        var result = await _eventService.GetById(id);
        return result.Success ? Ok(EventDto.FromEvent(result.Data)) : result.ToErrorResponse();
    }
    
    [HttpPost]
    [AccessGroupFilter(UserGroup.EventAdmin)]
    public async Task<ActionResult<EventDto>> CreateEvent([FromBody] CreateEventDto createEventDto)
    {
        var userId = RequestUserId();
        var result = await _eventService.CreateEventAsync(createEventDto, userId);
        return result.Success ? Ok(EventDto.FromEvent(result.Data)) : result.ToErrorResponse();
    }
}
