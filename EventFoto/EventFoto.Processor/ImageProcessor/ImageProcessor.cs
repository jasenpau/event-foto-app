using EventFoto.Core.Watermarks;
using EventFoto.Data.BlobStorage;
using EventFoto.Data.Models;
using EventFoto.Data.Repositories;
using ExifLibrary;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace EventFoto.Processor.ImageProcessor;

public class ImageProcessor : IImageProcessor
{
    private readonly IBlobStorage _blobStorage;
    private readonly IEventPhotoRepository _photoRepository;
    private readonly IUploadBatchRepository _uploadBatchRepository;
    private readonly IWatermarkService _watermarkService;
    private readonly IConfiguration _configuration;

    private const int MarginPixels = 20;
    private const float DefaultOpacity = 0.8f;

    public ImageProcessor(IBlobStorage blobStorage,
        IEventPhotoRepository photoRepository,
        IUploadBatchRepository uploadBatchRepository,
        IWatermarkService watermarkService,
        IConfiguration configuration)
    {
        _blobStorage = blobStorage;
        _photoRepository = photoRepository;
        _uploadBatchRepository = uploadBatchRepository;
        _watermarkService = watermarkService;
        _configuration = configuration;
    }

    public async Task<int> ProcessImagesAsync(ProcessingMessage message, CancellationToken cancellationToken = default)
    {
        var uploadBatch = await _uploadBatchRepository.GetByIdAsync(message.EntityId);
        if (uploadBatch == null)
        {
            throw new InvalidOperationException("Upload batch not found");
        }

        var photos = uploadBatch.EventPhotos;
        var count = await ProcessImagesAsync(photos, cancellationToken);
        await _uploadBatchRepository.MarkAsReadyAsync(uploadBatch.Id);
        return count;
    }

    public async Task<int> ReprocessEventImages(ProcessingMessage message, CancellationToken cancellationToken = default)
    {
        var eventId = message.EntityId;
        var eventPhotos = await _photoRepository.GetEventPhotosAsync(eventId);
        var count = await ProcessImagesAsync(eventPhotos, cancellationToken);
        return count;
    }

    public async Task<int> ReprocessGalleryImages(ProcessingMessage message, CancellationToken cancellationToken = default)
    {
        var galleryId = message.EntityId;
        var galleryPhotos = await _photoRepository.GetGalleryPhotosAsync(galleryId);
        var count = await ProcessImagesAsync(galleryPhotos, cancellationToken);
        return count;
    }


    private async Task<int> ProcessImagesAsync(IList<EventPhoto> messagePhotos, CancellationToken cancellationToken = default)
    {
        var processedCount = 0;

        var eventBatches = messagePhotos.GroupBy(p => p.Gallery.EventId);
        foreach (var eventPhotoBatch in eventBatches)
        {
            var eventId = eventPhotoBatch.Key;
            var containerName = _blobStorage.GetContainerName(eventId);

            var eventWatermarkImageId = eventPhotoBatch.First().Gallery.Event.WatermarkId;
            Image? eventWatermarkImage = null;
            if (eventWatermarkImageId.HasValue)
            {
                var eventWatermarkStream =
                    await _watermarkService.GetWatermarkFileAsync(eventWatermarkImageId.Value, cancellationToken);
                eventWatermarkImage = await Image.LoadAsync(eventWatermarkStream.Data, cancellationToken);
            }

            var galleryBatches = eventPhotoBatch.GroupBy(p => p.GalleryId);
            foreach (var galleryBatch in galleryBatches)
            {
                var galleryWatermarkId = galleryBatch.First().Gallery.WatermarkId;
                Image? galleryWatermarkImage = null;
                if (galleryWatermarkId.HasValue)
                {
                    var galleryWatermarkStream =
                        await _watermarkService.GetWatermarkFileAsync(galleryWatermarkId.Value, cancellationToken);
                    if (galleryWatermarkStream.Success)
                    {
                        galleryWatermarkImage = await Image.LoadAsync(galleryWatermarkStream.Data, cancellationToken);
                    }
                }

                foreach (var photo in galleryBatch)
                {
                    var watermarkId = galleryWatermarkId ?? eventWatermarkImageId;
                    var watermarkImage = galleryWatermarkImage ?? eventWatermarkImage;

                    if (photo.IsProcessed && watermarkId == photo.WatermarkId) continue;

                    var originalFilename = photo.Filename;
                    var imageStream =
                        await _blobStorage.DownloadFileAsync(containerName, originalFilename, cancellationToken);
                    if (!imageStream.Success)
                    {
                        throw new InvalidOperationException("Could not load image.");
                    }

                    using var image = await Image.LoadAsync(imageStream.Data, cancellationToken);
                    await imageStream.Data.DisposeAsync();

                    var processedFilename = $"out-{originalFilename}";

                    // Add image watermark processing
                    if (watermarkImage != null) ApplyWatermark(image, watermarkImage);

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

                    await _blobStorage.UploadFileAsync(containerName, processedFilename, outputStream,
                        cancellationToken);
                    await _photoRepository.MarkAsProcessed(eventPhoto, processedFilename, watermarkId);
                    processedCount++;
                }
            }
        }

        return processedCount;
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

    private static void ApplyWatermark(Image sourceImage, Image watermark, float opacity = DefaultOpacity)
    {
        var maxWatermarkWidth = sourceImage.Width / 4;
        var maxWatermarkHeight = sourceImage.Height / 4;

        var scale = 1.0f;
        if (watermark.Width > maxWatermarkWidth || watermark.Height > maxWatermarkHeight)
        {
            var scaleX = (float)maxWatermarkWidth / watermark.Width;
            var scaleY = (float)maxWatermarkHeight / watermark.Height;
            scale = Math.Min(scaleX, scaleY);
        }

        var newWidth = (int)(watermark.Width * scale);
        var newHeight = (int)(watermark.Height * scale);

        var posX = sourceImage.Width - newWidth - MarginPixels;
        var posY = sourceImage.Height - newHeight - MarginPixels;

        watermark.Mutate(x => x.Resize(newWidth, newHeight));
        sourceImage.Mutate(x => x.DrawImage(watermark, new Point(posX, posY), opacity));
    }
}
