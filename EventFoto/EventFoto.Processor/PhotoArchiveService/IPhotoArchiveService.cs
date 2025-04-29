using EventFoto.Data.Models;

namespace EventFoto.Processor.PhotoArchiveService;

public interface IPhotoArchiveService
{
    public Task ArchiveImagesAsync(string archiveFilename, IList<EventPhoto> photos, bool useProcessedPhotos,
        int? quality, CancellationToken cancellationToken);
}
