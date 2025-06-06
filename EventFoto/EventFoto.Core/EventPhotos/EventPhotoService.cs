﻿using System.Net;
using EventFoto.Core.Events;
using EventFoto.Core.Processing;
using EventFoto.Data.BlobStorage;
using EventFoto.Data.DTOs;
using EventFoto.Data.Enums;
using EventFoto.Data.Models;
using EventFoto.Data.Repositories;
using Microsoft.Extensions.Configuration;

namespace EventFoto.Core.EventPhotos;

public class EventPhotoService : IEventPhotoService
{
    private readonly IConfiguration _configuration;
    private readonly IEventRepository  _eventRepository;
    private readonly IBlobStorage _blobStorage;
    private readonly IEventPhotoRepository _eventPhotoRepository;
    private readonly IProcessingQueue _processingQueue;
    private readonly IDownloadRequestRepository _downloadRequestRepository;
    private readonly IGalleryRepository _galleryRepository;
    private readonly IUploadBatchRepository _uploadBatchRepository;
    private readonly IPhotographerAssignmentRepository _assignmentRepository;

    public EventPhotoService(IEventRepository eventRepository,
        IBlobStorage blobStorage,
        IEventPhotoRepository  eventPhotoRepository,
        IProcessingQueue processingQueue,
        IDownloadRequestRepository downloadRequestRepository,
        IGalleryRepository galleryRepository,
        IUploadBatchRepository uploadBatchRepository,
        IPhotographerAssignmentRepository assignmentRepository,
        IConfiguration configuration)
    {
        _eventRepository = eventRepository;
        _blobStorage = blobStorage;
        _eventPhotoRepository = eventPhotoRepository;
        _processingQueue = processingQueue;
        _downloadRequestRepository = downloadRequestRepository;
        _galleryRepository = galleryRepository;
        _uploadBatchRepository = uploadBatchRepository;
        _assignmentRepository = assignmentRepository;
        _configuration = configuration;
    }

    public async Task<ServiceResult<EventPhoto>> GetByIdAsync(int photoId)
    {
        var result = await _eventPhotoRepository.GetByIdAsync(photoId);
        return result is not null
            ? ServiceResult<EventPhoto>.Ok(result)
            : ServiceResult<EventPhoto>.Fail("Photo not found", HttpStatusCode.NotFound);
    }

    public async Task<ServiceResult<UploadBatch>> UploadPhotoBatch(Guid userId, UploadMessageDto uploadPhotoData)
    {
        var eventData = await _eventRepository.GetByIdAsync(uploadPhotoData.EventId);
        if (eventData is null)
        {
            return ServiceResult<UploadBatch>.Fail("Event not found", HttpStatusCode.NotFound);
        }

        var uploadBatch = await _uploadBatchRepository.CreateAsync(new UploadBatch
        {
            UserId = userId,
        });

        var galleryId = uploadPhotoData.GalleryId;
        if (galleryId is null)
        {
            var assignment = await _assignmentRepository.GetForEventAsync(uploadPhotoData.EventId, userId);
            galleryId = assignment?.GalleryId ?? eventData.DefaultGalleryId;
        }

        var eventPhotos = uploadPhotoData.PhotoFilenames.Select(photo => new EventPhoto
        {
            UserId = userId,
            CaptureDate = uploadPhotoData.CaptureDate,
            GalleryId = galleryId.Value,
            Filename = photo,
            UploadDate = DateTime.UtcNow,
            UploadBatchId = uploadBatch.Id,
        }).ToList();

        await _eventPhotoRepository.AddEventPhotosAsync(eventPhotos);
        await _processingQueue.EnqueueMessage(new ProcessingMessage
        {
            Type = ProcessingMessageType.UploadBatch,
            EntityId = uploadBatch.Id,
        });
        return ServiceResult<UploadBatch>.Ok(uploadBatch);
    }

    public async Task<ServiceResult<UploadBatch>> GetUploadBatchById(int batchId)
    {
        var batch = await _uploadBatchRepository.GetByIdAsync(batchId);
        return batch is null
            ? ServiceResult<UploadBatch>.Fail("Batch not found", HttpStatusCode.NotFound)
            : ServiceResult<UploadBatch>.Ok(batch);
    }

    public async Task<ServiceResult<SasUriResponseDto>> GetUploadSasUri(int eventId)
    {
        var eventData = await _eventRepository.GetByIdAsync(eventId);
        if (eventData is null)
        {
            return ServiceResult<SasUriResponseDto>.Fail("Event not found", HttpStatusCode.NotFound);
        }

        var tokenExpiryInMinutes = int.Parse(_configuration["AzureStorage:TokenExpiryInMinutes"] ?? "20");
        var containerName = _blobStorage.GetContainerName(eventId);
        var sasResult = await _blobStorage.GetUploadSasUri(containerName, tokenExpiryInMinutes);
        if (!sasResult.Success)
        {
            return ServiceResult<SasUriResponseDto>.Fail(sasResult);
        }

        var result = new SasUriResponseDto
        {
            SasUri = sasResult.Data,
            ExpiresOn = DateTime.UtcNow.AddMinutes(tokenExpiryInMinutes),
            EventId = eventId
        };
        return ServiceResult<SasUriResponseDto>.Ok(result);
    }

    public ServiceResult<SasUriResponseDto> GetReadOnlySasUri()
    {
        var tokenExpiryInMinutes = int.Parse(_configuration["AzureStorage:TokenExpiryInMinutes"] ?? "20");
        var sasUriResult = _blobStorage.GetReadOnlySasUri(tokenExpiryInMinutes);
        if (!sasUriResult.Success)
            return ServiceResult<SasUriResponseDto>.Fail(sasUriResult);

        var result = new SasUriResponseDto
        {
            SasUri = sasUriResult.Data,
            ExpiresOn = DateTime.UtcNow.AddMinutes(tokenExpiryInMinutes),
        };
        return ServiceResult<SasUriResponseDto>.Ok(result);
    }

    public async Task<ServiceResult<PagedData<string, EventPhoto>>> SearchPhotosAsync(
        EventPhotoSearchParams searchParams)

    {
        var result = await _eventPhotoRepository.SearchPhotosAsync(searchParams);
        return result is not null
            ? ServiceResult<PagedData<string, EventPhoto>>.Ok(result)
            : ServiceResult<PagedData<string, EventPhoto>>.Fail("Query failed", HttpStatusCode.InternalServerError);
    }

    public async Task<ServiceResult<int>> DeletePhotosAsync(IList<int> photoIds, CancellationToken cancellationToken)
    {
        var photos = await _eventPhotoRepository.GetByIdsAsync(photoIds);
        if (photos.Count == 0)
            return ServiceResult<int>.Ok(0);

        var eventGroups = photos.GroupBy(x => x.Gallery.EventId);
        foreach (var eventGroup in eventGroups)
        {
            var filenames = new List<string>();
            filenames.AddRange(eventGroup.Select(x => x.Filename));
            filenames.AddRange(eventGroup
                .Select(x => x.ProcessedFilename)
                .Where(x => !string.IsNullOrEmpty(x)));
            filenames.AddRange(eventGroup
                .Where(x => x.IsProcessed)
                .Select(x => $"thumb-{x.ProcessedFilename}"));

            var containerName = _blobStorage.GetContainerName(eventGroup.Key);
            await _blobStorage.DeleteFilesAsync(containerName, filenames, cancellationToken);
        }

        await _eventPhotoRepository.DeleteEventPhotosAsync(photos, cancellationToken);
        return ServiceResult<int>.Ok(photos.Count);
    }

    public async Task<ServiceResult<DownloadRequest>> DownloadPhotosAsync(Guid userId, IList<int> photoIds, bool processed, int? quality)
    {
        var createDate = DateTime.UtcNow;
        var request = new DownloadRequest
        {
            UserId = userId,
            Filename = createDate.ToString("yyyy-MM-dd HHmmss") + ".zip",
            DownloadProcessedPhotos = processed,
            Quality = quality,
            DownloadImages = photoIds.Select(p => new DownloadImage
            {
                EventPhotoId = p
            }).ToList()
        };
        var downloadRequest = await _downloadRequestRepository.CreateAsync(request);

        await _processingQueue.EnqueueMessage(new ProcessingMessage
        {
            Type = ProcessingMessageType.DownloadZip,
            EntityId = downloadRequest.Id,
            Filename = request.Filename,
        });

        return ServiceResult<DownloadRequest>.Ok(downloadRequest);
    }

    public async Task<ServiceResult<DownloadRequest>> GetDownloadRequestAsync(Guid userId, int requestId)
    {
        var request = await _downloadRequestRepository.GetByIdAsync(requestId);
        if (request is null || request.UserId != userId)
            return ServiceResult<DownloadRequest>.Fail("Download request not found", HttpStatusCode.NotFound);

        return ServiceResult<DownloadRequest>.Ok(request);
    }

    public async Task<ServiceResult<int>> MovePhotos(IList<int> photoIds, int galleryId)
    {
        var destinationGallery = await _galleryRepository.GetByIdAsync(galleryId);
        if (destinationGallery == null)
        {
            return ServiceResult<int>.Fail($"Gallery with ID {galleryId} does not exist", HttpStatusCode.NotFound);
        }

        var photos = await _eventPhotoRepository.GetByIdsAsync(photoIds);
        foreach (var photo in photos)
        {
            photo.GalleryId = destinationGallery.Id;
            photo.Gallery = destinationGallery;
        }

        await _eventPhotoRepository.UpdateEventPhotosAsync(photos);
        return ServiceResult<int>.Ok(photos.Count);
    }
}
