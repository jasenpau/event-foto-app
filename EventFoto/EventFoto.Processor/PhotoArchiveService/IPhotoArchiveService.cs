using EventFoto.Data.Models;

namespace EventFoto.Processor.PhotoArchiveService;

public interface IPhotoArchiveService
{
    public Task ArchiveImagesAsync(string archiveFilename, IList<EventPhoto> photos, bool useProcessedPhotos,
        CancellationToken cancellationToken);
}
