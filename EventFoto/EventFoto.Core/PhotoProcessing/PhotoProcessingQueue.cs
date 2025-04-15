using System.Text.Json;
using Azure.Storage.Queues;
using EventFoto.Data.Models;
using Microsoft.Extensions.Configuration;

namespace EventFoto.Core.PhotoProcessing;

public class PhotoProcessingQueue : IPhotoProcessingQueue
{
    private readonly string _connectionString;
    private readonly string _queueName;

    public PhotoProcessingQueue(IConfiguration configuration)
    {
        _connectionString = configuration["AzureStorage:ConnectionString"] ??
                            throw new ArgumentNullException(nameof(configuration));
        _queueName = configuration["AzureStorage:ProcessingQueueName"] ??
                     throw new ArgumentNullException(nameof(configuration));
    }

    public async Task EnqueuePhotoAsync(ProcessingMessage message)
    {
        var client = new QueueClient(_connectionString, _queueName);
        await client.CreateIfNotExistsAsync();

        if (await client.ExistsAsync())
        {
            message.EnqueuedOn = DateTime.UtcNow;
            var serialized = JsonSerializer.Serialize(message);
            var base64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(serialized));
            await client.SendMessageAsync(base64);
        }
        else
        {
            throw new InvalidOperationException($"Queue '{_queueName}' does not exist.");
        }
    }
}
