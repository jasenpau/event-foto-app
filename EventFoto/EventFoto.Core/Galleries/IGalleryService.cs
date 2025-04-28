using EventFoto.Data.DatabaseProjections;
using EventFoto.Data.DTOs;
using EventFoto.Data.Models;

namespace EventFoto.Core.Galleries;

public interface IGalleryService
{
    public Task<ServiceResult<Gallery>> GetGalleryAsync(int id);
    public Task<ServiceResult<bool>> DeleteGalleryAsync(int id);
    public Task<ServiceResult<Gallery>> UpdateGalleryAsync(int id, CreateEditGalleryRequestDto galleryDto);
    public Task<ServiceResult<Gallery>> CreateGalleryAsync(int eventId, string name, int? watermarkId);
    public Task<ServiceResult<List<EventGalleryProjection>>> GetGalleriesAsync(int eventId);
}
