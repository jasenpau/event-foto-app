using System.Data.Common;
using System.Net;
using EventFoto.Data.DTOs;
using EventFoto.Data.Models;
using EventFoto.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace EventFoto.Core.Events;

public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;

    public EventService(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<ServiceResult<Event>> GetById(int id)
    {
        var result = await _eventRepository.GetByIdAsync(id);
        if (result is not null) return ServiceResult<Event>.Ok(result);
        return ServiceResult<Event>.Fail("Event not found", HttpStatusCode.NotFound);
    }

    public async Task<ServiceResult<IList<Event>>> GetAllEventsByUserAsync(Guid userId)
    {
        var events = await _eventRepository.GetAllEventsByUserAsync(userId);
        return ServiceResult<IList<Event>>.Ok(events);
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
            return ServiceResult<Event>.Ok(eventData);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException { SqlState: "23505" })
            {
                return ServiceResult<Event>.Fail("Duplicate event title", HttpStatusCode.Conflict);
            }
            else
            {
                throw;
            }
        }
    }
}
