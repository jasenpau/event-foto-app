﻿using EventFoto.Data.Models;
using EventFoto.Data.PhotoStorage;
using EventFoto.Data.Repositories;
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
        var imageStream =
            await _photoBlobStorage.DownloadImageAsync(containerName, message.Filename, cancellationToken);
        if (!imageStream.Success)
        {
            throw new InvalidOperationException("Could not download image.");
        }

        var image = await Image.LoadAsync(imageStream.Data, cancellationToken);
        await imageStream.Data.DisposeAsync();
        var eventPhoto = await _photoRepository.GetByEventAndFilename(message.EventId, message.Filename);

        var thumbnailTask = GeneratePreviewImage(image, message, cancellationToken);

        await Task.WhenAll(thumbnailTask);
        await _photoRepository.MarkAsProcessed(eventPhoto, message.Filename);
    }

    private async Task GeneratePreviewImage(Image originalImage, ProcessingMessage message,
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

        var content = new MultipartFormDataContent
        {
            { new StreamContent(thumbnailStream), "thumbnail", $"thumb-{message.Filename}" },
            { new StringContent(message.EventId.ToString()), "eventId" }
        };

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", _configuration["ImageProcessorOptions:AuthorizationKey"]);
        client.BaseAddress = new Uri(_configuration["ImageProcessorOptions:ThumbnailUploadEndpoint"] ??
                                     throw new InvalidOperationException("Missing ThumbnailUploadEndpoint"));

        var response = await client.PostAsync("/api/image/thumbnail", content, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
