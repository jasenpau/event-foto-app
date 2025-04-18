using System.Text.Json;
using Azure.Storage.Queues.Models;
using EventFoto.Data.Models;
using EventFoto.Processor.ImageProcessor;
using Microsoft.Azure.Functions.Worker;

namespace EventFoto.Processor.Functions;

public class ProcessImageFunction
{
    private readonly IImageProcessor _imageProcessor;

    public ProcessImageFunction(IImageProcessor imageProcessor)
    {
        _imageProcessor = imageProcessor;
    }

    [Function("ProcessImage")]
    public async Task ProcessImage(
        [QueueTrigger("%ProcessingQueueName%", Connection = "QueueConnectionString")] QueueMessage queueMessage,
        FunctionContext context, CancellationToken cancellationToken = default)

    {
        var message = JsonSerializer.Deserialize<ProcessingMessage>(queueMessage.MessageText);
        if (message == null) throw new InvalidOperationException("Invalid queue message");

        await _imageProcessor.ProcessImageAsync(message, cancellationToken);
    }
}
