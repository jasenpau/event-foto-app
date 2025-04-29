using System.IO.Compression;
using EventFoto.Data.BlobStorage;
using EventFoto.Data.Models;
using Microsoft.Extensions.Configuration;

namespace EventFoto.Processor.PhotoArchiveService;

public class PhotoArchiveService : IPhotoArchiveService
{
    private readonly IBlobStorage _blobStorage;
    private readonly IConfiguration _configuration;

    public PhotoArchiveService(IBlobStorage blobStorage, IConfiguration configuration)
    {
        _blobStorage = blobStorage;
        _configuration = configuration;
    }

    public async Task ArchiveImagesAsync(string archiveFilename, IList<EventPhoto> photos, bool useProcessedPhotos, CancellationToken cancellationToken)
    {
        var tempFilePath = Path.GetTempFileName();
        var outputZipPath = Path.ChangeExtension(tempFilePath, ".zip");

        // Compress images to zip
        await using (var zipStream = new FileStream(outputZipPath, FileMode.Create))
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: false))
        {
            foreach (var item in photos)
            {
                if (useProcessedPhotos && !item.IsProcessed) continue;

                var container = _blobStorage.GetContainerName(item.Gallery.EventId);
                var filename = useProcessedPhotos ? item.ProcessedFilename : item.Filename;

                var imageResult = await _blobStorage.DownloadFileAsync(container, filename, cancellationToken);
                if (!imageResult.Success) continue;
                await using var imageStream = imageResult.Data;

                var entry = archive.CreateEntry(filename, CompressionLevel.Optimal);
                await using var entryStream = entry.Open();
                await imageStream.CopyToAsync(entryStream, cancellationToken);
            }
        }

        // Upload zip to blob storage
        var zipFilename = archiveFilename;
        var archiveContainerName = _configuration["ProcessorOptions:ArchiveDownloadContainer"];
        await using (var finalZipStream = File.OpenRead(outputZipPath))
        {
            await _blobStorage.UploadFileAsync(archiveContainerName, zipFilename, finalZipStream, "application/zip",
                cancellationToken);
        }
        File.Delete(outputZipPath);
    }
}
