using EventFoto.Data.DTOs;
using EventFoto.Data.Models;

namespace EventFoto.Core.EventPhotoService;

public interface IEventPhotoService
{
    public Task<ServiceResult<EventPhoto>> UploadPhoto(EventPhotoUploadData photoData);
    public Task<ServiceResult<SasUriResponseDto>> GetUploadSasUri(int eventId);
}
