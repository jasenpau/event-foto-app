using System.Net;
using EventFoto.Core.Events;
using EventFoto.Data.DTOs;
using EventFoto.Data.Models;
using EventFoto.Data.PhotoStorage;
using Microsoft.Extensions.Configuration;

namespace EventFoto.Core.EventPhotoService;

public class EventPhotoService : IEventPhotoService
{
    private readonly IConfiguration _configuration;
    private readonly IEventService  _eventService;
    private readonly IPhotoBlobStorage _photoBlobStorage;

    public EventPhotoService(IEventService eventService,
        IPhotoBlobStorage photoBlobStorage,
        IConfiguration configuration)
    {
        _eventService = eventService;
        _photoBlobStorage = photoBlobStorage;
        _configuration = configuration;
    }

    public Task<ServiceResult<EventPhoto>> UploadPhoto(EventPhotoUploadData photoData)
    {

        throw new NotImplementedException();
    }

    public async Task<ServiceResult<SasUriResponseDto>> GetUploadSasUri(int eventId)
    {
        var eventResult = await _eventService.GetById(eventId);
        if (!eventResult.Success)
        {
            return ServiceResult<SasUriResponseDto>.Fail(eventResult.ErrorMessage,
                eventResult.StatusCode ?? HttpStatusCode.NotFound);
        }

        var tokenExpiryInMinutes = int.Parse(_configuration["PhotoBlob:TokenExpiryInMinutes"] ?? "20");
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
