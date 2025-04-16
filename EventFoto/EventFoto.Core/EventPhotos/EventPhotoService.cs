using System.Net;
using EventFoto.Core.Events;
using EventFoto.Core.PhotoProcessing;
using EventFoto.Data.DTOs;
using EventFoto.Data.Models;
using EventFoto.Data.PhotoStorage;
using EventFoto.Data.Repositories;
using Microsoft.Extensions.Configuration;

namespace EventFoto.Core.EventPhotos;

public class EventPhotoService : IEventPhotoService
{
    private readonly IConfiguration _configuration;
    private readonly IEventService  _eventService;
    private readonly IPhotoBlobStorage _photoBlobStorage;
    private readonly IEventPhotoRepository _eventPhotoRepository;
    private readonly IPhotoProcessingQueue _processingQueue;

    public EventPhotoService(IEventService eventService,
        IPhotoBlobStorage photoBlobStorage,
        IEventPhotoRepository  eventPhotoRepository,
        IPhotoProcessingQueue processingQueue,
        IConfiguration configuration)
    {
        _eventService = eventService;
        _photoBlobStorage = photoBlobStorage;
        _eventPhotoRepository = eventPhotoRepository;
        _processingQueue = processingQueue;
        _configuration = configuration;
    }

    public async Task<ServiceResult<EventPhoto>> GetByIdAsync(int photoId)
    {
        var result = await _eventPhotoRepository.GetByIdAsync(photoId);
        return result is not null
            ? ServiceResult<EventPhoto>.Ok(result)
            : ServiceResult<EventPhoto>.Fail("Photo not found", HttpStatusCode.NotFound);
    }

    public async Task<ServiceResult<EventPhoto>> UploadPhoto(Guid userId, UploadMessageDto uploadPhotoData)
    {
        var eventPhoto = new EventPhoto
        {
            UserId = userId,
            CaptureDate = uploadPhotoData.CaptureDate,
            EventId = uploadPhotoData.EventId,
            Filename = uploadPhotoData.Filename,
            UploadDate = DateTime.UtcNow,
        };

        await _eventPhotoRepository.AddEventPhotoAsync(eventPhoto);
        await _processingQueue.EnqueuePhotoAsync(new ProcessingMessage
        {
            EventId = uploadPhotoData.EventId,
            Filename = uploadPhotoData.Filename,
        });
        return ServiceResult<EventPhoto>.Ok(eventPhoto);
    }

    public async Task<ServiceResult<SasUriResponseDto>> GetUploadSasUri(int eventId)
    {
        var eventResult = await _eventService.GetById(eventId);
        if (!eventResult.Success)
        {
            return ServiceResult<SasUriResponseDto>.Fail(eventResult.ErrorMessage,
                eventResult.StatusCode ?? HttpStatusCode.NotFound);
        }

        var tokenExpiryInMinutes = int.Parse(_configuration["AzureStorage:TokenExpiryInMinutes"] ?? "20");
        var containerName = _photoBlobStorage.GetContainerName(eventId);
        var sasResult = await _photoBlobStorage.GetUploadSasUri(containerName, tokenExpiryInMinutes);
        if (!sasResult.Success)
        {
            return ServiceResult<SasUriResponseDto>.Fail(sasResult.ErrorMessage,
                sasResult.StatusCode ?? HttpStatusCode.InternalServerError);
        }

        var result = new SasUriResponseDto
        {
            SasUri = sasResult.Data,
            ExpiresOn = DateTime.UtcNow.AddMinutes(tokenExpiryInMinutes),
            EventId = eventId
        };
        return ServiceResult<SasUriResponseDto>.Ok(result);
    }

    public async Task<ServiceResult<PagedData<string, EventPhoto>>> SearchEventPhotosAsync(
        EventPhotoSearchParams searchParams)

    {
        var result = await _eventPhotoRepository.SearchEventPhotosAsync(searchParams);
        return result is not null
            ? ServiceResult<PagedData<string, EventPhoto>>.Ok(result)
            : ServiceResult<PagedData<string, EventPhoto>>.Fail("Query failed", HttpStatusCode.InternalServerError);
    }

    public async Task<ServiceResult<string>> SaveThumbnail(int eventId, string contentRootPath, string fileName, MemoryStream thumbStream)
    {
        var thumbnailsDir = Path.Combine(contentRootPath, "Thumbnails", eventId.ToString());

        if (!Directory.Exists(thumbnailsDir))
            Directory.CreateDirectory(thumbnailsDir);

        var filePath = Path.Combine(thumbnailsDir, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await thumbStream.CopyToAsync(stream);
        await thumbStream.DisposeAsync();
        return ServiceResult<string>.Ok(filePath);
    }

    public async Task<ServiceResult<MemoryStream>> GetRawPhotoAsync(int eventId, string filename, CancellationToken cancellationToken)
    {
        var containerName = _photoBlobStorage.GetContainerName(eventId);
        var streamResult = await _photoBlobStorage.DownloadImageAsync(containerName, filename, cancellationToken);
        return streamResult;
    }

}
