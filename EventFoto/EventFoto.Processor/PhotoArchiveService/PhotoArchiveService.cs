using System.IO.Compression;
using EventFoto.Data.BlobStorage;
using EventFoto.Data.Models;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;

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

    public async Task ArchiveImagesAsync(string archiveFilename, IList<EventPhoto> photos, bool useProcessedPhotos,
        int? quality, CancellationToken cancellationToken)

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

                if (quality is not null)
                {
                    await using var entryStream = entry.Open();
                    await Resize(imageStream, entryStream, quality.Value);
                }
                else
                {
                    await using var entryStream = entry.Open();
                    await imageStream.CopyToAsync(entryStream, cancellationToken);
                }
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

    private static async Task Resize(Stream inputStream, Stream outputStream, int maxLongEdge)
    {
        using var image = await Image.LoadAsync(inputStream);
        var width = image.Width;
        var height = image.Height;
        var format = image.Metadata.DecodedImageFormat;

        if (format == null)
        {
            throw new InvalidOperationException("Image format not supported");
        }

        var longEdge = Math.Max(width, height);
        if (longEdge > maxLongEdge)
        {
            var scale = (double)maxLongEdge / longEdge;
            var newWidth = (int)(width * scale);
            var newHeight = (int)(height * scale);

            image.Mutate(x => x.Resize(newWidth, newHeight));
        }

        await image.SaveAsync(outputStream, format);
    }
}
