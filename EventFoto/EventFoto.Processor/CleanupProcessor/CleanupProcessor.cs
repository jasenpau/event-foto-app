using EventFoto.Data.BlobStorage;
using EventFoto.Data.Repositories;
using Microsoft.Extensions.Configuration;

namespace EventFoto.Processor.CleanupProcessor;

public class CleanupProcessor : ICleanupProcessor
{
    private readonly IDownloadRequestRepository _downloadRequestRepository;
    private readonly IGalleryRepository _galleryRepository;
    private readonly IUploadBatchRepository _uploadBatchRepository;
    private readonly IBlobStorage _blobStorage;
    private readonly IConfiguration _configuration;

    public CleanupProcessor(IDownloadRequestRepository downloadRequestRepository,
        IGalleryRepository galleryRepository,
        IUploadBatchRepository uploadBatchRepository,
        IBlobStorage blobStorage,
        IConfiguration configuration)
    {
        _downloadRequestRepository = downloadRequestRepository;
        _galleryRepository = galleryRepository;
        _uploadBatchRepository = uploadBatchRepository;
        _blobStorage = blobStorage;
        _configuration = configuration;
    }

    public async Task<CleanupResult> CleanupAsync(DateTime executionDateTime, CancellationToken cancellationToken)
    {
        var deletionDate = executionDateTime.AddDays(-1);
        var downloadRequests = await _downloadRequestRepository.GetBeforeDate(deletionDate);
        var blobs = downloadRequests.Select(r => r.Filename).ToList();
        var archiveContainerName = _configuration["ProcessorOptions:ArchiveDownloadContainer"];

        await _downloadRequestRepository.DeleteAsync(downloadRequests);
        var blobDeletionResult = await _blobStorage.DeleteFilesAsync(archiveContainerName, blobs, cancellationToken);
        var uploadBatchesDeleted = await _uploadBatchRepository.DeleteBeforeDateAsync(deletionDate);

        return new CleanupResult
        {
            DownloadRequests = downloadRequests.Count,
            DownloadZips = blobDeletionResult.Success ? blobDeletionResult.Data : 0,
            UploadBatches = uploadBatchesDeleted
        };
    }
}
