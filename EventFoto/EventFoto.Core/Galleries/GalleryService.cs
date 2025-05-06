using System.Net;
using EventFoto.Core.Processing;
using EventFoto.Data.DatabaseProjections;
using EventFoto.Data.DTOs;
using EventFoto.Data.Enums;
using EventFoto.Data.Models;
using EventFoto.Data.Repositories;

namespace EventFoto.Core.Galleries;

public class GalleryService : IGalleryService
{
    private readonly IGalleryRepository _galleryRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IEventPhotoRepository _eventPhotoRepository;
    private readonly IProcessingQueue _processingQueue;

    public GalleryService(
        IGalleryRepository galleryRepository,
        IEventRepository eventRepository,
        IEventPhotoRepository eventPhotoRepository,
        IProcessingQueue processingQueue)
    {
        _galleryRepository = galleryRepository;
        _eventRepository = eventRepository;
        _eventPhotoRepository = eventPhotoRepository;
        _processingQueue = processingQueue;
    }

    public async Task<ServiceResult<Gallery>> GetGalleryAsync(int id)
    {
        var result = await _galleryRepository.GetByIdAsync(id);
        return result == null
            ? ServiceResult<Gallery>.Fail($"Gallery with ID {id} not found", HttpStatusCode.NotFound)
            : ServiceResult<Gallery>.Ok(result);
    }

    public async Task<ServiceResult<bool>> DeleteGalleryAsync(int id)
    {
        var gallery = await _galleryRepository.GetByIdAsync(id);
        if (gallery == null)
        {
            return ServiceResult<bool>.Fail($"Gallery with ID {id} not found", HttpStatusCode.NotFound);
        }

        if (gallery.Event.DefaultGalleryId == id)
        {
            return ServiceResult<bool>.Fail("Cannot delete default gallery for this event", "default-gallery",
                HttpStatusCode.BadRequest);
        }

        if (await _galleryRepository.HasPhotosAsync(id))
        {
            return ServiceResult<bool>.Fail("Cannot delete gallery with photos", "not-empty-gallery",
                HttpStatusCode.BadRequest);
        }

        var result = await _galleryRepository.DeleteAsync(id);
        return ServiceResult<bool>.Ok(result);
    }

    public async Task<ServiceResult<Gallery>> UpdateGalleryAsync(int id, CreateEditGalleryRequestDto galleryDto)
    {
        var gallery = await _galleryRepository.GetByIdAsync(id);
        if (gallery == null) return ServiceResult<Gallery>.Fail($"Gallery with ID {id} not found", HttpStatusCode.NotFound);

        var nameExists = await _galleryRepository.NameExistsAsync(galleryDto.Name, gallery.EventId, id);
        if (nameExists)
        {
            return ServiceResult<Gallery>.Fail($"Event already has gallery with given name", HttpStatusCode.Conflict);
        }

        gallery.Name = galleryDto.Name.Trim();
        gallery.WatermarkId = galleryDto.WatermarkId;
        var result = await _galleryRepository.UpdateAsync(gallery);

        if (galleryDto.ReprocessPhotos)
        {
            await _processingQueue.EnqueueMessage(new ProcessingMessage
            {
                Type = ProcessingMessageType.ReprocessGallery,
                EntityId = id
            });
        }

        return ServiceResult<Gallery>.Ok(result);
    }

    public async Task<ServiceResult<Gallery>> CreateGalleryAsync(int eventId, string name, int? watermarkId)
    {
        var eventData = await _eventRepository.GetByIdAsync(eventId);
        if (eventData == null)
        {
            return ServiceResult<Gallery>.Fail($"Event with ID {eventId} not found", HttpStatusCode.NotFound);
        }

        var nameExists = await _galleryRepository.NameExistsAsync(name, eventId, null);
        if (nameExists)
        {
            return ServiceResult<Gallery>.Fail($"Event already has gallery with given name", HttpStatusCode.Conflict);
        }

        var gallery = new Gallery
        {
            EventId = eventId,
            Name = name,
            WatermarkId = watermarkId
        };

        var createdGallery = await _galleryRepository.CreateAsync(gallery);
        return ServiceResult<Gallery>.Ok(createdGallery);
    }

    public async Task<ServiceResult<List<EventGalleryProjection>>> GetGalleriesAsync(int eventId)
    {
        var galleries = await _galleryRepository.GetByEventIdAsync(eventId);
        return ServiceResult<List<EventGalleryProjection>>.Ok(galleries);
    }
}
