using Azure.Storage.Queues;

namespace EventFoto.Core.Processing;

public interface IQueueClientFactory
{
    public QueueClient GetClient();
}
