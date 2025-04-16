using EventFoto.Data.DTOs;
using EventFoto.Data.Models;

namespace EventFoto.Data.Repositories;

public interface IEventPhotoRepository
{
    public Task<EventPhoto> GetByIdAsync(int photoId);
    public Task<EventPhoto> AddEventPhotoAsync(EventPhoto eventPhoto);
    public Task<EventPhoto> MarkAsProcessed(EventPhoto eventPhoto, string processedFilename);
    public Task<EventPhoto> GetByEventAndFilename(int eventId, string filename);
    public Task<PagedData<string, EventPhoto>> SearchEventPhotosAsync(EventPhotoSearchParams searchParams);
    public Task<List<EventPhoto>> GetByIdsAsync(IList<int> photoIds);
    public Task DeleteEventPhotosAsync(IList<EventPhoto> photos, CancellationToken cancellationToken);
}
