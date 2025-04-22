using EventFoto.Data.BlobStorage;
using EventFoto.Data.Repositories;
using Microsoft.Extensions.Configuration;

namespace EventFoto.Processor.CleanupProcessor;

public class CleanupProcessor : ICleanupProcessor
{
    private readonly IDownloadRequestRepository _downloadRequestRepository;
    private readonly IBlobStorage _blobStorage;
    private readonly IConfiguration _configuration;

    public CleanupProcessor(IDownloadRequestRepository downloadRequestRepository,
        IBlobStorage blobStorage,
        IConfiguration configuration)
    {
        _downloadRequestRepository = downloadRequestRepository;
        _blobStorage = blobStorage;
        _configuration = configuration;
    }

    public async Task<int> CleanupAsync(DateTime executionDateTime, CancellationToken cancellationToken)
    {
        var deletionDate = executionDateTime.AddDays(-1);
        var downloadRequests = await _downloadRequestRepository.GetBeforeDate(deletionDate);
        var blobs = downloadRequests.Select(r => r.Filename).ToList();
        var archiveContainerName = _configuration["ProcessorOptions:ArchiveDownloadContainer"];

        var blobDeletionTask = _blobStorage.DeleteFilesAsync(archiveContainerName, blobs, cancellationToken);
        var dbDeletionTask = _downloadRequestRepository.DeleteAsync(downloadRequests);

        await Task.WhenAll(blobDeletionTask, dbDeletionTask);
        return downloadRequests.Count;
    }
}
