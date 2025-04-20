using EventFoto.Data.DTOs;
using EventFoto.Data.Models;

namespace EventFoto.Core.EventPhotos;

public interface IEventPhotoService
{
    public Task<ServiceResult<EventPhoto>> GetByIdAsync(int photoId);
    public Task<ServiceResult<EventPhoto>> UploadPhoto(Guid userId, UploadMessageDto uploadPhotoData);
    public Task<ServiceResult<SasUriResponseDto>> GetUploadSasUri(int eventId);
    public ServiceResult<SasUriResponseDto> GetReadOnlySasUri();
    public Task<ServiceResult<PagedData<string, EventPhoto>>> SearchEventPhotosAsync(
        EventPhotoSearchParams searchParams);
    public Task<ServiceResult<MemoryStream>> GetPhotoFromBlobAsync(int eventId, string filename, CancellationToken cancellationToken);
    public Task<ServiceResult<int>> DeletePhotosAsync(IList<int> photoIds, CancellationToken cancellationToken);
}
