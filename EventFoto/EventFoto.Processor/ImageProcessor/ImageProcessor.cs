using EventFoto.Data.BlobStorage;
using EventFoto.Data.Models;
using EventFoto.Data.Repositories;
using ExifLibrary;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace EventFoto.Processor.ImageProcessor;

public class ImageProcessor : IImageProcessor
{
    private readonly IBlobStorage _blobStorage;
    private readonly IEventPhotoRepository _photoRepository;
    private readonly IUploadBatchRepository _uploadBatchRepository;
    private readonly IConfiguration _configuration;

    public ImageProcessor(IBlobStorage blobStorage,
        IEventPhotoRepository photoRepository,
        IUploadBatchRepository uploadBatchRepository,
        IConfiguration configuration)
    {
        _blobStorage = blobStorage;
        _photoRepository = photoRepository;
        _uploadBatchRepository = uploadBatchRepository;
        _configuration = configuration;
    }

    public async Task ProcessImagesAsync(ProcessingMessage message, CancellationToken cancellationToken = default)
    {
        var uploadBatch = await _uploadBatchRepository.GetByIdAsync(message.EntityId);
        if (uploadBatch == null)
        {
            throw new InvalidOperationException("Upload batch not found.");
        }

        var eventBatches = uploadBatch.EventPhotos.GroupBy(p => p.Gallery.EventId);
        foreach (var eventPhotoBatch in eventBatches)
        {
            var  eventId = eventPhotoBatch.Key;
            var containerName = _blobStorage.GetContainerName(eventId);

            foreach (var photo in eventPhotoBatch)
            {
                var originalFilename = photo.Filename;
                var imageStream = await _blobStorage.DownloadFileAsync(containerName, originalFilename, cancellationToken);
                if (!imageStream.Success)
                {
                    throw new InvalidOperationException("Could not load image.");
                }

                using var image = await Image.LoadAsync(imageStream.Data, cancellationToken);
                await imageStream.Data.DisposeAsync();

                var processedFilename = $"out-{originalFilename}";

                // Add image watermark processing here
                // ...

                // Thumbnail processing
                await GeneratePreviewImage(image, eventId, processedFilename, cancellationToken);

                // Output processing
                using var processedImageStream = new MemoryStream();
                await image.SaveAsJpegAsync(processedImageStream, cancellationToken);
                processedImageStream.Position = 0;

                // Add EXIF metadata
                var eventPhoto = await _photoRepository.GetByEventAndFilename(eventId, originalFilename);
                var outputFile = await AddExifData(eventPhoto, processedImageStream);

                // Save final image to stream
                using var outputStream = new MemoryStream();
                await outputFile.SaveAsync(outputStream);
                outputStream.Position = 0;

                await _blobStorage.UploadFileAsync(containerName, processedFilename, outputStream, cancellationToken);
                await _photoRepository.MarkAsProcessed(eventPhoto, processedFilename);
            }
        }

        await _uploadBatchRepository.MarkAsReadyAsync(uploadBatch.Id);
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
        var thumbnailSize = int.Parse(_configuration["ProcessorOptions:ThumbnailSize"] ?? "500");
        var scaleFactor = originalImage.Width > originalImage.Height
            ? (double)originalImage.Height / thumbnailSize
            : (double)originalImage.Width / thumbnailSize;

        var thumbnailHeight = (int)(originalImage.Height / scaleFactor);
        var thumbnailWidth = (int)(originalImage.Width / scaleFactor);

        using var thumbnail = originalImage.Clone(x => x.Resize(thumbnailWidth, thumbnailHeight));
        var thumbnailStream = new MemoryStream();
        await thumbnail.SaveAsJpegAsync(thumbnailStream, cancellationToken);
        thumbnailStream.Position = 0;

        var containerName = _blobStorage.GetContainerName(eventId);
        var thumbnailFilename = $"thumb-{filename}";
        await _blobStorage.UploadFileAsync(containerName, thumbnailFilename, thumbnailStream, cancellationToken);
    }
}
