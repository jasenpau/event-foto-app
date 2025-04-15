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
            ExpiresOn = DateTime.UtcNow.AddMinutes(tokenExpiryInMinutes)
        };
        return ServiceResult<SasUriResponseDto>.Ok(result);
    }
}
