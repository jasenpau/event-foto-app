using System.Net;
using EventFoto.Data.BlobStorage;
using EventFoto.Data.DTOs;
using EventFoto.Data.Models;
using EventFoto.Data.Repositories;
using Microsoft.Extensions.Configuration;

namespace EventFoto.Core.Watermarks;

public class WatermarkService : IWatermarkService
{
    private readonly IWatermarkRepository _repository;
    private readonly IBlobStorage _blobStorage;
    private readonly IConfiguration _configuration;
    private readonly IEventRepository _eventRepository;
    private readonly IGalleryRepository _galleryRepository;

    public WatermarkService(IWatermarkRepository repository,
        IBlobStorage blobStorage,
        IConfiguration configuration,
        IEventRepository eventRepository,
        IGalleryRepository galleryRepository)
    {
        _repository = repository;
        _blobStorage = blobStorage;
        _configuration = configuration;
        _eventRepository = eventRepository;
        _galleryRepository = galleryRepository;
    }

    public async Task<ServiceResult<Watermark>> UploadWatermarkAsync(string name, MemoryStream imageData, CancellationToken cancellationToken)
    {
        var containerName = _configuration["AzureStorage:WatermarkContainer"];
        var generatedFilename = $"{Guid.NewGuid()}.png";

        var uploadResult = await _blobStorage.UploadFileAsync(containerName, generatedFilename, imageData, "image/png", cancellationToken);

        if (!uploadResult.Success)
            return ServiceResult<Watermark>.Fail(uploadResult);

        var watermark = new Watermark
        {
            Name = name,
            Filename = generatedFilename
        };

        var createdWatermark = await _repository.CreateWatermarkAsync(watermark);
        return ServiceResult<Watermark>.Ok(createdWatermark);
    }

    public async Task<ServiceResult<bool>> DeleteWatermarkAsync(int id, CancellationToken cancellationToken)
    {
        var watermark = await _repository.GetWatermarkAsync(id);
        if (watermark == null)
            return ServiceResult<bool>.Fail("Watermark not found.", HttpStatusCode.NotFound);

        var containerName = _configuration["AzureStorage:WatermarkContainer"];
        var deleteResult = await _blobStorage.DeleteFilesAsync(containerName, new List<string> { watermark.Filename }, cancellationToken);

        if (!deleteResult.Success)
            return ServiceResult<bool>.Fail(deleteResult);

        await _repository.DeleteWatermarkAsync(id);
        return ServiceResult<bool>.Ok(true);
    }

    public ServiceResult<Watermark> GetWatermarkAsync(int id)
    {
        var watermark = _repository.GetWatermarkAsync(id).Result;
        return watermark == null
            ? ServiceResult<Watermark>.Fail("Watermark not found.", HttpStatusCode.NotFound)
            : ServiceResult<Watermark>.Ok(watermark);
    }

    public ServiceResult<PagedData<string, Watermark>> SearchWatermarksAsync(WatermarkSearchParams searchParams)
    {
        var results = _repository.SearchWatermarksAsync(searchParams).Result;
        return ServiceResult<PagedData<string, Watermark>>.Ok(results);
    }

    public async Task<ServiceResult<MemoryStream>> GetWatermarkFileForEventAsync(int eventId, CancellationToken cancellationToken)
    {
        var eventData = await _eventRepository.GetByIdAsync(eventId);
        if (eventData == null)
            return ServiceResult<MemoryStream>.Fail("Event not found.", HttpStatusCode.NotFound);

        if (eventData.WatermarkId == null)
            return ServiceResult<MemoryStream>.Fail("No watermark assigned to this event.", HttpStatusCode.NoContent);

        var watermark = await _repository.GetWatermarkAsync(eventData.WatermarkId.Value);
        if (watermark == null)
            return ServiceResult<MemoryStream>.Fail("Watermark not found.", HttpStatusCode.NotFound);

        var containerName = _configuration["AzureStorage:WatermarkContainer"];
        return await _blobStorage.DownloadFileAsync(containerName, watermark.Filename, cancellationToken);
    }

    public async Task<ServiceResult<MemoryStream>> GetWatermarkFileForGalleryAsync(int galleryId, CancellationToken cancellationToken)
    {
        var eventData = await _galleryRepository.GetByIdAsync(galleryId);
        if (eventData == null)
            return ServiceResult<MemoryStream>.Fail("Gallery not found.", HttpStatusCode.NotFound);

        if (eventData.WatermarkId == null)
            return ServiceResult<MemoryStream>.Fail("No watermark assigned to this gallery.", HttpStatusCode.NoContent);

        var watermark = await _repository.GetWatermarkAsync(eventData.WatermarkId.Value);
        if (watermark == null)
            return ServiceResult<MemoryStream>.Fail("Watermark not found.", HttpStatusCode.NotFound);

        var containerName = _configuration["AzureStorage:WatermarkContainer"];
        return await _blobStorage.DownloadFileAsync(containerName, watermark.Filename, cancellationToken);
    }
}
