using EventFoto.Data.Models;

namespace EventFoto.Core.PhotoProcessing;

public interface IPhotoProcessingQueue
{
    public Task EnqueuePhotoAsync(ProcessingMessage message);
}
