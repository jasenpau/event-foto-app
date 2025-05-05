using System.Text.Json;
using EventFoto.Data.Models;

namespace EventFoto.Core.Processing;

public class ProcessingQueue : IProcessingQueue
{
    private readonly IQueueClientFactory _queueClientFactory;

    public ProcessingQueue(IQueueClientFactory queueClientFactory)
    {
        _queueClientFactory = queueClientFactory;
    }

    public async Task EnqueueMessage(ProcessingMessage message)
    {
        var client = _queueClientFactory.GetClient();
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
            throw new InvalidOperationException("Queue does not exist.");
        }
    }
}
