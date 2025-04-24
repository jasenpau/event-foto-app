using EventFoto.Data.DTOs;
using EventFoto.Data.Models;

namespace EventFoto.Core.EventPhotos;

public interface IEventPhotoService
{
    public Task<ServiceResult<EventPhoto>> GetByIdAsync(int photoId);
    public Task<ServiceResult<UploadBatch>> UploadPhotoBatch(Guid userId, UploadMessageDto uploadPhotoData);
    public Task<ServiceResult<UploadBatch>> GetUploadBatchById(int batchId);
    public Task<ServiceResult<SasUriResponseDto>> GetUploadSasUri(int eventId);
    public ServiceResult<SasUriResponseDto> GetReadOnlySasUri();
    public Task<ServiceResult<PagedData<string, EventPhoto>>> SearchPhotosAsync(EventPhotoSearchParams searchParams);
    public Task<ServiceResult<int>> DeletePhotosAsync(IList<int> photoIds, CancellationToken cancellationToken);
    public Task<ServiceResult<DownloadRequest>> DownloadPhotosAsync(Guid userId, IList<int> photoIds);
    public Task<ServiceResult<DownloadRequest>> GetDownloadRequestAsync(Guid userId, int requestId);
    public Task<ServiceResult<int>> MovePhotos(IList<int> photoIds, int galleryId);
}
