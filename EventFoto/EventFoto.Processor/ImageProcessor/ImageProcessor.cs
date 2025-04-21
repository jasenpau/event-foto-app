using EventFoto.Data.Models;
using EventFoto.Data.PhotoStorage;
using EventFoto.Data.Repositories;
using ExifLibrary;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace EventFoto.Processor.ImageProcessor;

public class ImageProcessor : IImageProcessor
{
    private readonly IPhotoBlobStorage _photoBlobStorage;
    private readonly IEventPhotoRepository _photoRepository;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public ImageProcessor(IPhotoBlobStorage photoBlobStorage,
        IEventPhotoRepository photoRepository,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _photoBlobStorage = photoBlobStorage;
        _photoRepository = photoRepository;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    public async Task ProcessImageAsync(ProcessingMessage message, CancellationToken cancellationToken = default)
    {
        var containerName = _photoBlobStorage.GetContainerName(message.EventId);
        var imageStream = await _photoBlobStorage.DownloadImageAsync(containerName, message.Filename, cancellationToken);
        if (!imageStream.Success)
        {
            throw new InvalidOperationException("Could not load image.");
        }

        using var image = await Image.LoadAsync(imageStream.Data, cancellationToken);
        await imageStream.Data.DisposeAsync();

        var processedFilename = $"out-{message.Filename}";

        // Add image watermark processing here
        // ...

        // Thumbnail processing
        await GeneratePreviewImage(image, message.EventId, processedFilename, cancellationToken);

        // Output processing
        using var processedImageStream = new MemoryStream();
        await image.SaveAsJpegAsync(processedImageStream, cancellationToken);
        processedImageStream.Position = 0;

        // Add EXIF metadata
        var eventPhoto = await _photoRepository.GetByEventAndFilename(message.EventId, message.Filename);
        var outputFile = await AddExifData(eventPhoto, processedImageStream);

        // Save final image to stream
        using var outputStream = new MemoryStream();
        await outputFile.SaveAsync(outputStream);
        outputStream.Position = 0;

        await _photoBlobStorage.UploadImageAsync(containerName, processedFilename, outputStream, cancellationToken);
        await _photoRepository.MarkAsProcessed(eventPhoto, processedFilename);
    }

    private async Task<ImageFile> AddExifData(EventPhoto photo, MemoryStream imageStream)
    {
        var file = await ImageFile.FromStreamAsync(imageStream);
        file.Properties.Set(ExifTag.DateTimeOriginal, photo.CaptureDate);
        file.Properties.Set(ExifTag.Artist, photo.User.Name);
        return file;
    }

    private async Task GeneratePreviewImage(Image originalImage, int eventId, string filename,
        CancellationToken cancellationToken)

    {
        var thumbnailSize = int.Parse(_configuration["ImageProcessorOptions:ThumbnailSize"] ?? "500");
        var scaleFactor = originalImage.Width > originalImage.Height
            ? (double)originalImage.Height / thumbnailSize
            : (double)originalImage.Width / thumbnailSize;

        var thumbnailHeight = (int)(originalImage.Height / scaleFactor);
        var thumbnailWidth = (int)(originalImage.Width / scaleFactor);

        using var thumbnail = originalImage.Clone(x => x.Resize(thumbnailWidth, thumbnailHeight));
        var thumbnailStream = new MemoryStream();
        await thumbnail.SaveAsJpegAsync(thumbnailStream, cancellationToken);
        thumbnailStream.Position = 0;

        var containerName = _photoBlobStorage.GetContainerName(eventId);
        var thumbnailFilename = $"thumb-{filename}";
        await _photoBlobStorage.UploadImageAsync(containerName, thumbnailFilename, thumbnailStream, cancellationToken);
    }
}
