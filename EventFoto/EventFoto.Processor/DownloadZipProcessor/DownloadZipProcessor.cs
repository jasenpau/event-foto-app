using System.IO.Compression;
using EventFoto.Data.BlobStorage;
using EventFoto.Data.Models;
using EventFoto.Data.Repositories;
using Microsoft.Extensions.Configuration;

namespace EventFoto.Processor.DownloadZipProcessor;

public class DownloadZipProcessor : IDownloadZipProcessor
{
    private readonly IDownloadRequestRepository _downloadRequestRepository;
    private readonly IBlobStorage _blobStorage;
    private readonly IConfiguration _configuration;

    public DownloadZipProcessor(IDownloadRequestRepository downloadRequestRepository,
        IBlobStorage blobStorage,
        IConfiguration configuration)
    {
        _downloadRequestRepository = downloadRequestRepository;
        _blobStorage = blobStorage;
        _configuration = configuration;
    }

    public async Task<int> ProcessDownloadAsync(ProcessingMessage message, CancellationToken cancellationToken)
    {
        var request = await _downloadRequestRepository.GetWithImagesAsync(message.EntityId);
        if (request == null) throw new InvalidOperationException("Download request not found");

        var tempFilePath = Path.GetTempFileName();
        var outputZipPath = Path.ChangeExtension(tempFilePath, ".zip");

        await using (var zipStream = new FileStream(outputZipPath, FileMode.Create))
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: false))
        {
            foreach (var item in request.DownloadImages)
            {
                if (!item.EventPhoto.IsProcessed) continue;

                var container = _blobStorage.GetContainerName(item.EventPhoto.Gallery.EventId);
                var filename = item.EventPhoto.ProcessedFilename;

                var imageResult = await _blobStorage.DownloadFileAsync(container, filename, cancellationToken);
                await using var imageStream = imageResult.Data;

                var entry = archive.CreateEntry(filename, CompressionLevel.Optimal);
                await using var entryStream = entry.Open();
                await imageStream.CopyToAsync(entryStream, cancellationToken);
            }
        }

        // Upload zip to blob storage
        var zipFilename = message.Filename;
        var archiveContainerName = _configuration["ProcessorOptions:ArchiveDownloadContainer"];
        await using (var finalZipStream = File.OpenRead(outputZipPath))
        {
            await _blobStorage.UploadFileAsync(archiveContainerName, zipFilename, finalZipStream, "application/zip",
                cancellationToken);
        }

        // Update the request
        await _downloadRequestRepository.MarkAsReady(request.Id);

        // Cleanup
        File.Delete(outputZipPath);

        return request.DownloadImages.Count;
    }
}
