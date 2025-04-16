using EventFoto.Data.DTOs;
using EventFoto.Data.Models;

namespace EventFoto.Core.EventPhotos;

public interface IEventPhotoService
{
    public Task<ServiceResult<EventPhoto>> UploadPhoto(Guid userId, UploadMessageDto uploadPhotoData);
    public Task<ServiceResult<SasUriResponseDto>> GetUploadSasUri(int eventId);

    public Task<ServiceResult<PagedData<string, EventPhoto>>> SearchEventPhotosAsync(
        EventPhotoSearchParams searchParams);
}
