using System.Net;
using EventFoto.Data.DatabaseProjections;
using EventFoto.Data.DTOs;
using EventFoto.Data.Models;
using EventFoto.Data.Repositories;

namespace EventFoto.Core.Galleries;

public class GalleryService : IGalleryService
{
    private readonly IGalleryRepository _galleryRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IEventPhotoRepository _eventPhotoRepository;

    public GalleryService(
        IGalleryRepository galleryRepository,
        IEventRepository eventRepository,
        IEventPhotoRepository eventPhotoRepository)
    {
        _galleryRepository = galleryRepository;
        _eventRepository = eventRepository;
        _eventPhotoRepository = eventPhotoRepository;
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

        if (gallery.Photos != null && gallery.Photos.Any())
        {
            await _eventPhotoRepository.MoveAllGalleryPhotosAsync(id, gallery.Event.DefaultGalleryId);
        }

        var result = await _galleryRepository.DeleteAsync(id);
        if (!result)
        {
            return ServiceResult<bool>.Fail("Failed to delete gallery", HttpStatusCode.InternalServerError);
        }

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<Gallery>> UpdateGalleryAsync(int id, string name)
    {
        var gallery = await _galleryRepository.GetByIdAsync(id);
        if (gallery == null) return ServiceResult<Gallery>.Fail($"Gallery with ID {id} not found");

        gallery.Name = name;
        var result = await _galleryRepository.UpdateAsync(gallery);
        return ServiceResult<Gallery>.Ok(result);
    }

    public async Task<ServiceResult<Gallery>> CreateGalleryAsync(int eventId, string name)
    {
        var eventData = await _eventRepository.GetByIdAsync(eventId);
        if (eventData == null)
        {
            return ServiceResult<Gallery>.Fail($"Event with ID {eventId} not found", HttpStatusCode.NotFound);
        }

        var nameExists = await _galleryRepository.NameExistsAsync(name, eventId);
        if (nameExists)
        {
            return ServiceResult<Gallery>.Fail($"Event already has gallery with given name", HttpStatusCode.Conflict);
        }

        var gallery = new Gallery
        {
            EventId = eventId,
            Name = name,
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
