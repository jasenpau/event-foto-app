using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;

namespace EventFoto.Core.Processing;

public class QueueClientFactory : IQueueClientFactory
{
    private readonly string _connectionString;
    private readonly string _queueName;

    public QueueClientFactory(IConfiguration configuration)
    {
        _connectionString = configuration["AzureStorage:ConnectionString"] ??
                            throw new ArgumentNullException(nameof(configuration));
        _queueName = configuration["AzureStorage:ProcessingQueueName"] ??
                     throw new ArgumentNullException(nameof(configuration));
    }

    public QueueClient GetClient()
    {
        return new QueueClient(_connectionString, _queueName);
    }
}
