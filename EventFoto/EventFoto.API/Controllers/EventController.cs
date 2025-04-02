using EventFoto.API.Extensions;
using EventFoto.API.Filters;
using EventFoto.Core.Events;
using EventFoto.Data.DTOs;
using EventFoto.Data.Enums;
using EventFoto.Data.Models;
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
        var events = await _eventService.GetAllEventsAsync();
        var eventDtos = events.Data.Select(EventListDto.FromEvent).ToArray();
        return Ok(eventDtos);
    }

    [HttpGet("search")]
    public async Task<ActionResult<PagedData<string, EventListDto>>> SearchEvents([FromQuery] EventSearchParams searchParams)
    {
        var searchResult = await _eventService.SearchEventsAsync(searchParams);
        if (!searchResult.Success)
        {
            return searchResult.ToErrorResponse();
        }

        var result = searchResult.Data.ToDto(EventListDto.FromEvent);
        return Ok(result);
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

    [HttpGet("{eventId:int}/photographers")]
    public async Task<ActionResult<IList<EventPhotographerDto>>> GetEventPhotographers(int eventId)
    {
        var result =  await _eventService.GetEventPhotographersAsync(eventId);
        return result.Success ? Ok(result.Data) : result.ToErrorResponse();
    }

    [HttpPost("{eventId:int}/photographers")]
    [AccessGroupFilter(UserGroup.Photographer)]
    public async Task<ActionResult<bool>> AssignPhotographer(int eventId, [FromBody] PhotographerAssignmentRequestDto requestDto)
    {
        var userGroup = RequestHighestUserGroup();
        if (userGroup is null || (userGroup == UserGroup.Photographer && requestDto.UserId != RequestUserId()))
        {
            return Unauthorized("User does not have permission to assign others.");
        }

        var result = await _eventService.AssignPhotographerAsync(eventId, requestDto.UserId);
        return result.Success ? Ok(result.Data) : result.ToErrorResponse();
    }

    [HttpDelete("{eventId:int}/photographers/{userId:guid}")]
    [AccessGroupFilter(UserGroup.Photographer)]
    public async Task<ActionResult<bool>> UnassignPhotographer(int eventId, Guid userId)
    {
        var userGroup = RequestHighestUserGroup();
        if (userGroup is null || (userGroup == UserGroup.Photographer && userId != RequestUserId()))
        {
            return Unauthorized("User does not have permission to unassign others.");
        }

        var result = await _eventService.UnassignPhotographerAsync(eventId, userId);
        return result.Success ? Ok(result.Data) : result.ToErrorResponse();
    }
}
