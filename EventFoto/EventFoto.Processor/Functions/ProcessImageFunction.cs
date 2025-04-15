using System.Text.Json;
using EventFoto.Core.EventPhotos;
using EventFoto.Data.Models;
using EventFoto.Data.PhotoStorage;
using EventFoto.Processor.ImageProcessor;
using Microsoft.AspNetCore.Http;
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
        [QueueTrigger("%ProcessingQueueName%", Connection = "QueueConnectionString")] string queueMessage,
        FunctionContext context, CancellationToken cancellationToken = default)

    {
        var message = JsonSerializer.Deserialize<ProcessingMessage>(queueMessage);
        if (message == null) throw new InvalidOperationException("Invalid queue message");

        await _imageProcessor.ProcessImageAsync(message, cancellationToken);
    }
}
