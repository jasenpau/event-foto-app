using EventFoto.Data.BlobStorage;
using EventFoto.Data.Models;
using EventFoto.Data.Repositories;
using EventFoto.Processor.PhotoArchiveService;

namespace EventFoto.Processor.EventArchiveProcessor;

public class EventArchiveProcessor : IEventArchiveProcessor
{
    private readonly IEventRepository _eventRepository;
    private readonly IEventPhotoRepository _eventPhotoRepository;
    private readonly IBlobStorage _blobStorage;
    private readonly IPhotoArchiveService _photoArchiveService;

    public EventArchiveProcessor(IEventRepository eventRepository,
        IEventPhotoRepository eventPhotoRepository,
        IBlobStorage blobStorage,
        IPhotoArchiveService photoArchiveService)
    {
        _eventRepository = eventRepository;
        _eventPhotoRepository = eventPhotoRepository;
        _blobStorage = blobStorage;
        _photoArchiveService = photoArchiveService;
    }

    public async Task<int> ArchiveEventAsync(ProcessingMessage message, CancellationToken cancellationToken)
    {
        var eventId = message.EntityId;
        var eventData = await _eventRepository.GetByIdAsync(eventId);

        // Create zip
        var eventPhotos = await _eventPhotoRepository.GetAllEventPhotosAsync(eventId);
        await _photoArchiveService.ArchiveImagesAsync(message.Filename, eventPhotos, true,
            cancellationToken);

        // Delete photos & container
        var container = _blobStorage.GetContainerName(eventId);
        await _blobStorage.DeleteContainerAsync(container, cancellationToken);
        await _eventPhotoRepository.DeleteEventPhotosAsync(eventPhotos, cancellationToken);

        eventData.IsArchived = true;
        await _eventRepository.UpdateAsync(eventData);

        return eventPhotos.Count;
    }
}
