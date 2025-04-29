using EventFoto.Data.Models;
using EventFoto.Data.Repositories;
using EventFoto.Processor.PhotoArchiveService;

namespace EventFoto.Processor.DownloadZipProcessor;

public class DownloadZipProcessor : IDownloadZipProcessor
{
    private readonly IDownloadRequestRepository _downloadRequestRepository;
    private readonly IPhotoArchiveService _photoArchiveService;

    public DownloadZipProcessor(IDownloadRequestRepository downloadRequestRepository,
        IPhotoArchiveService photoArchiveService)
    {
        _downloadRequestRepository = downloadRequestRepository;
        _photoArchiveService = photoArchiveService;
    }

    public async Task<int> ProcessDownloadAsync(ProcessingMessage message, CancellationToken cancellationToken)
    {
        var request = await _downloadRequestRepository.GetWithImagesAsync(message.EntityId);
        if (request == null) throw new InvalidOperationException("Download request not found");

        // Compress photos
        var photos = request.DownloadImages.Select(i => i.EventPhoto).ToList();
        await _photoArchiveService.ArchiveImagesAsync(message.Filename, photos, request.DownloadProcessedPhotos, cancellationToken);

        // Update the request
        await _downloadRequestRepository.MarkAsReady(request.Id);

        return request.DownloadImages.Count;
    }
}
