using EventFoto.Core.ControllerBase;
using EventFoto.Core.Events;
using EventFoto.Core.Extensions;
using EventFoto.Core.Filters;
using EventFoto.Data.DTOs;
using EventFoto.Data.Enums;
using EventFoto.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventFoto.Core.Assignments;

namespace EventFoto.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EventController : AppControllerBase
{
    private readonly IEventService _eventService;
    private readonly IAssignmentService _assignmentService;

    public EventController(IEventService eventService, IAssignmentService assignmentService)
    {
        _eventService = eventService;
        _assignmentService = assignmentService;
    }

    [HttpGet("search")]
    public async Task<ActionResult<PagedData<string, EventListDto>>> SearchEvents([FromQuery] EventSearchParams searchParams)
    {
        var searchResult = await _eventService.SearchEventsAsync(searchParams);
        if (!searchResult.Success)
        {
            return searchResult.ToErrorResponse();
        }

        var result = searchResult.Data.ToDto(EventListDto.FromProjection);
        return Ok(result);
    }
    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<EventDto>> GetEventById(int id)
    {
        var result = await _eventService.GetById(id);
        return result.Success ? Ok(EventDto.FromEvent(result.Data)) : result.ToErrorResponse();
    }
    
    [HttpPost]
    [AccessGroupFilter(UserGroup.EventAdmin)]
    public async Task<ActionResult<EventDto>> CreateEvent([FromBody] CreateEditEventDto createEventDto)
    {
        var userId = RequestUserId();
        var result = await _eventService.CreateEventAsync(createEventDto, userId);
        return result.Success ? Ok(EventDto.FromEvent(result.Data)) : result.ToErrorResponse();
    }

    [HttpPut("{id:int}")]
    [AccessGroupFilter(UserGroup.EventAdmin)]
    public async Task<ActionResult<EventDto>> UpdateEvent(int id, [FromBody] CreateEditEventDto updateEventDto)
    {
        var result = await _eventService.UpdateEventAsync(id, updateEventDto);
        return result.Success ? Ok(EventDto.FromEvent(result.Data)) : result.ToErrorResponse();
    }

    [HttpGet("{eventId:int}/photographers")]
    public async Task<ActionResult<IList<AssignmentResponseDto>>> GetEventPhotographers(int eventId)
    {
        var result = await _assignmentService.GetAssignmentsForEvent(eventId);
        if (!result.Success)
        {
            return result.ToErrorResponse();
        }

        var response = result.Data.Select(AssignmentResponseDto.FromModel).ToList();
        return Ok(response);
    }

    [HttpPost("{eventId:int}/photographers")]
    [AccessGroupFilter(UserGroup.Photographer)]
    public async Task<ActionResult<bool>> AssignPhotographer(int eventId, [FromBody] AssignmentRequestDto requestDto)
    {
        var userGroup = RequestHighestUserGroup();
        if (userGroup is null || (userGroup == UserGroup.Photographer && requestDto.UserId != RequestUserId()))
        {
            return Unauthorized("User does not have permission to assign others.");
        }

        var result = await _assignmentService.AssignPhotographerAsync(requestDto.GalleryId, requestDto.UserId);
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

        var result = await _assignmentService.RemovePhotographerAssignmentAsync(eventId, userId);
        return result.Success ? Ok(result.Data) : result.ToErrorResponse();
    }
}
