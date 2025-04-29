using System.Text.Json;
using Azure.Storage.Queues.Models;
using EventFoto.Data.Enums;
using EventFoto.Data.Models;
using EventFoto.Processor.DownloadZipProcessor;
using EventFoto.Processor.EventArchiveProcessor;
using EventFoto.Processor.ImageProcessor;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace EventFoto.Processor.Functions;

public class ProcessImageFunction
{
    private readonly IImageProcessor _imageProcessor;
    private readonly IDownloadZipProcessor _downloadZipProcessor;
    private readonly IEventArchiveProcessor _eventArchiveProcessor;
    private readonly ILogger _logger;

    public ProcessImageFunction(IImageProcessor imageProcessor,
        IDownloadZipProcessor downloadZipProcessor,
        IEventArchiveProcessor eventArchiveProcessor,
        ILogger<ProcessImageFunction> logger)
    {
        _imageProcessor = imageProcessor;
        _downloadZipProcessor = downloadZipProcessor;
        _eventArchiveProcessor = eventArchiveProcessor;
        _logger = logger;
    }

    [Function("ProcessMessage")]
    public async Task ProcessMessage(
        [QueueTrigger("%AzureStorage:ProcessingQueueName%", Connection = "AzureStorage:ConnectionString")] QueueMessage queueMessage,
        FunctionContext context, CancellationToken cancellationToken = default)

    {
        var message = JsonSerializer.Deserialize<ProcessingMessage>(queueMessage.MessageText);
        if (message == null) throw new InvalidOperationException("Invalid queue message");

        var count = message.Type switch
        {
            ProcessingMessageType.UploadBatch => await _imageProcessor.ProcessImagesAsync(message, cancellationToken),
            ProcessingMessageType.DownloadZip => await _downloadZipProcessor.ProcessDownloadAsync(message, cancellationToken),
            ProcessingMessageType.ReprocessEvent => await _imageProcessor.ReprocessEventImages(message, cancellationToken),
            ProcessingMessageType.ReprocessGallery => await _imageProcessor.ReprocessGalleryImages(message, cancellationToken),
            ProcessingMessageType.ArchiveEvent => await _eventArchiveProcessor.ArchiveEventAsync(message, cancellationToken),
            _ => throw new InvalidOperationException("Invalid message type")
        };
        _logger.Log(LogLevel.Information,
            $"Processed message: {typeof(ProcessingMessageType).GetEnumName(message.Type)} " +
            $"for ID: {message.EntityId}, enqueued on {message.EnqueuedOn:yyyy-MM-dd HH:mm:ss}. " +
            $"Entries processed: {count}");
    }
}
