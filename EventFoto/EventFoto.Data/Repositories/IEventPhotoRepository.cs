using EventFoto.Data.DTOs;
using EventFoto.Data.Models;

namespace EventFoto.Data.Repositories;

public interface IEventPhotoRepository
{
    public Task<EventPhoto> GetByIdAsync(int photoId);
    public Task<List<EventPhoto>> GetGalleryPhotosAsync(int galleryId);
    public Task<List<EventPhoto>> GetEventPhotosAsync(int eventId);
    public Task<IList<EventPhoto>> AddEventPhotosAsync(IList<EventPhoto> eventPhotos);
    public Task<EventPhoto> MarkAsProcessed(EventPhoto eventPhoto, string processedFilename, int? watermarkId = null);
    public Task<EventPhoto> GetByEventAndFilename(int eventId, string filename);
    public Task<PagedData<string, EventPhoto>> SearchPhotosAsync(EventPhotoSearchParams searchParams);
    public Task<List<EventPhoto>> GetByIdsAsync(IList<int> photoIds);
    public Task DeleteEventPhotosAsync(IList<EventPhoto> photos, CancellationToken cancellationToken);
    public Task UpdateEventPhotosAsync(IList<EventPhoto> photos);
    public Task<List<EventPhoto>> GetAllEventPhotosAsync(int eventId);
}
