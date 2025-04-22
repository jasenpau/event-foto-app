using EventFoto.Data.Models;

namespace EventFoto.Core.Processing;

public interface IProcessingQueue
{
    public Task EnqueueMessage(ProcessingMessage message);
}
