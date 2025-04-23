using System.Net;
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
            return ServiceResult<bool>.Fail("Cannot delete default gallery for this event", HttpStatusCode.BadRequest);
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
}
