using System.Data.Common;
using System.Net;
using EventFoto.Data.DTOs;
using EventFoto.Data.Extensions;
using EventFoto.Data.Models;
using EventFoto.Data.PhotoStorage;
using EventFoto.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace EventFoto.Core.Events;

public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPhotoBlobStorage _photoBlobStorage;

    public EventService(IEventRepository eventRepository,
        IUserRepository userRepository,
        IPhotoBlobStorage  photoBlobStorage)
    {
        _eventRepository = eventRepository;
        _userRepository = userRepository;
        _photoBlobStorage = photoBlobStorage;
    }

    public async Task<ServiceResult<Event>> GetById(int id)
    {
        var result = await _eventRepository.GetByIdAsync(id);
        return result is not null
            ? ServiceResult<Event>.Ok(result)
            : ServiceResult<Event>.Fail("Event not found", HttpStatusCode.NotFound);
    }

    public async Task<ServiceResult<IList<EventPhotographerDto>>> GetEventPhotographersAsync(int eventId)
    {
        var photographerUsers = await _eventRepository.GetEventPhotographersAsync(eventId);
        if (photographerUsers is null)
            return ServiceResult<IList<EventPhotographerDto>>.Fail("Event not found", HttpStatusCode.NotFound);

        var photographers = MapAssignedPhotographersToDto(photographerUsers);
        return ServiceResult<IList<EventPhotographerDto>>.Ok(photographers);
    }

    public async Task<ServiceResult<IList<EventPhotographerDto>>> AssignPhotographerAsync(int eventId, Guid userId)
    {
        var eventData = await _eventRepository.GetByIdWithPhotographersAsync(eventId);
        if (eventData is null)
            return ServiceResult<IList<EventPhotographerDto>>.Fail("Event not found", HttpStatusCode.NotFound);

        if (eventData.Photographers.Any(u => u.Id == userId))
        {
            return ServiceResult<IList<EventPhotographerDto>>.Fail("User is already assigned", HttpStatusCode.Conflict);
        }

        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user is null)
            return ServiceResult<IList<EventPhotographerDto>>.Fail("User not found", HttpStatusCode.NotFound);

        eventData.Photographers.Add(user);
        await _eventRepository.UpdateEventAsync(eventData);
        return ServiceResult<IList<EventPhotographerDto>>.Ok(MapAssignedPhotographersToDto(eventData.Photographers));
    }

    public async Task<ServiceResult<IList<EventPhotographerDto>>> UnassignPhotographerAsync(int eventId, Guid userId)
    {
        var eventData = await _eventRepository.GetByIdWithPhotographersAsync(eventId);
        if (eventData is null)
            return ServiceResult<IList<EventPhotographerDto>>.Fail("Event not found", HttpStatusCode.NotFound);

        var userIndex = eventData.Photographers.FindIndex(u => u.Id == userId);
        if (userIndex == -1)
            return ServiceResult<IList<EventPhotographerDto>>.Fail("User is not assigned", HttpStatusCode.NotFound);


        eventData.Photographers.RemoveAt(userIndex);
        await _eventRepository.UpdateEventAsync(eventData);
        return ServiceResult<IList<EventPhotographerDto>>.Ok(MapAssignedPhotographersToDto(eventData.Photographers));

    }

    public async Task<ServiceResult<Event>> CreateEventAsync(CreateEventDto eventDto, Guid userId)
    {
        var eventData = new Event
        {
            Name = eventDto.Name.Trim(),
            StartDate = eventDto.StartDate,
            EndDate = eventDto.EndDate,
            Location = eventDto.Location?.Trim(),
            Note = eventDto.Note?.Trim(),
            CreatedBy = userId,
            IsArchived = false
        };
        try
        {
            eventData = await _eventRepository.CreateAsync(eventData);
            var containerName = _photoBlobStorage.GetContainerName(eventData.Id);
            await _photoBlobStorage.CreateContainerAsync(containerName);
            return ServiceResult<Event>.Ok(eventData);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException { SqlState: "23505" })
            {
                return ServiceResult<Event>.Fail("Duplicate event title", HttpStatusCode.Conflict);
            }

            throw;
        }
    }

    public async Task<ServiceResult<PagedData<string, Event>>> SearchEventsAsync(EventSearchParams searchParams)
    {
        var result = await _eventRepository.SearchEventsAsync(searchParams);
        return result is not null
            ? ServiceResult<PagedData<string, Event>>.Ok(result)
            : ServiceResult<PagedData<string, Event>>.Fail("Query failed", HttpStatusCode.InternalServerError);
    }

    private IList<EventPhotographerDto> MapAssignedPhotographersToDto(IList<User> photographers)
    {
        return photographers.Select(p => new EventPhotographerDto
        {
            Id = p.Id,
            Name = p.Name,
            PhotoCount = 0,
        }).ToList();
    }
}
