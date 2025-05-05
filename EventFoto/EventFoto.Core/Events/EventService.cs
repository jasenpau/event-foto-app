using System.Net;
using EventFoto.Core.EventPhotos;
using EventFoto.Core.Processing;
using EventFoto.Data.BlobStorage;
using EventFoto.Data.DatabaseProjections;
using EventFoto.Data.DTOs;
using EventFoto.Data.Enums;
using EventFoto.Data.Models;
using EventFoto.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace EventFoto.Core.Events;

public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;
    private readonly IBlobStorage _blobStorage;
    private readonly IProcessingQueue _processingQueue;
    private readonly IConfiguration _configuration;

    public EventService(IEventRepository eventRepository,
        IBlobStorage  blobStorage,
        IProcessingQueue processingQueue,
        IConfiguration configuration)
    {
        _eventRepository = eventRepository;
        _blobStorage = blobStorage;
        _processingQueue = processingQueue;
        _configuration = configuration;
    }

    public async Task<ServiceResult<Event>> GetById(int id)
    {
        var result = await _eventRepository.GetByIdAsync(id);
        return result is not null
            ? ServiceResult<Event>.Ok(result)
            : ServiceResult<Event>.Fail("Event not found", HttpStatusCode.NotFound);
    }

    public async Task<ServiceResult<Event>> CreateEventAsync(CreateEditEventDto eventDto, Guid userId)
    {
        var eventData = new Event
        {
            Name = eventDto.Name.Trim(),
            StartDate = eventDto.StartDate,
            EndDate = eventDto.EndDate,
            Location = eventDto.Location?.Trim(),
            Note = eventDto.Note?.Trim(),
            CreatedBy = userId,
            IsArchived = false,
            ArchiveName = null,
            WatermarkId = eventDto.WatermarkId,
        };
        var defaultGallery = new Gallery
        {
            Name = "Pagrindinė galerija",
        };
        try
        {
            eventData = await _eventRepository.CreateAsync(eventData, defaultGallery);
            var containerName = _blobStorage.GetContainerName(eventData.Id);
            await _blobStorage.CreateContainerAsync(containerName);
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

    public async Task<ServiceResult<Event>> UpdateEventAsync(int id, CreateEditEventDto eventDto)
    {
        try
        {
            var existingEvent = await _eventRepository.GetByIdAsync(id);
            if (existingEvent == null)
                return ServiceResult<Event>.Fail("Event not found", HttpStatusCode.NotFound);

            existingEvent.Name = eventDto.Name.Trim();
            existingEvent.StartDate = eventDto.StartDate;
            existingEvent.EndDate = eventDto.EndDate;
            existingEvent.Location = eventDto.Location?.Trim();
            existingEvent.Note = eventDto.Note?.Trim();
            existingEvent.WatermarkId = eventDto.WatermarkId;

            var updatedEvent = await _eventRepository.UpdateAsync(existingEvent);

            if (eventDto.ReprocessPhotos)
            {
                await _processingQueue.EnqueueMessage(new ProcessingMessage()
                {
                    Type = ProcessingMessageType.ReprocessEvent,
                    EntityId = updatedEvent.Id,
                });
            }

            return ServiceResult<Event>.Ok(updatedEvent);
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

    public async Task<ServiceResult<PagedData<string, EventListProjection>>> SearchEventsAsync(
        EventSearchParams searchParams)
    {
        var result = await _eventRepository.SearchEventsAsync(searchParams);
        return result is not null
            ? ServiceResult<PagedData<string, EventListProjection>>.Ok(result)
            : ServiceResult<PagedData<string, EventListProjection>>.Fail("Query failed",
                HttpStatusCode.InternalServerError);
    }

    public async Task<ServiceResult<string>> ArchiveEventAsync(int eventId)
    {
        var eventToArchive = await _eventRepository.GetByIdAsync(eventId);
        if (eventToArchive == null)
        {
            return ServiceResult<string>.Fail("Event not found", HttpStatusCode.NotFound);
        }

        eventToArchive.ArchiveName = $"renginio-archyvas-{eventId}.zip";
        await _eventRepository.UpdateAsync(eventToArchive);

        await _processingQueue.EnqueueMessage(new ProcessingMessage
        {
            Type = ProcessingMessageType.ArchiveEvent,
            EntityId = eventId,
            Filename = eventToArchive.ArchiveName
        });

        return ServiceResult<string>.Ok(eventToArchive.ArchiveName);
    }

    public async Task<ServiceResult<bool>> DeleteEventAsync(int id)
    {
        var eventToDelete = await _eventRepository.GetByIdAsync(id);
        if (eventToDelete == null)
        {
            return ServiceResult<bool>.Fail("Event not found", HttpStatusCode.NotFound);
        }

        if (!eventToDelete.IsArchived)
        {
            return ServiceResult<bool>.Fail("Only archived events can be deleted", HttpStatusCode.BadRequest);
        }

        if (!string.IsNullOrEmpty(eventToDelete.ArchiveName))
        {
            var containerName = _configuration["ProcessorOptions:ArchiveDownloadContainer"];
            await _blobStorage.DeleteFilesAsync(containerName, new List<string> { eventToDelete.ArchiveName },
                CancellationToken.None);
        }

        await _eventRepository.DeleteAsync(id);
        return ServiceResult<bool>.Ok(true);
    }
}
