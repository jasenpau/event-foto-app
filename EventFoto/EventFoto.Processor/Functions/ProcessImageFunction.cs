using System.Text.Json;
using Azure.Storage.Queues.Models;
using EventFoto.Data.Enums;
using EventFoto.Data.Models;
using EventFoto.Processor.DownloadZipProcessor;
using EventFoto.Processor.ImageProcessor;
using Microsoft.Azure.Functions.Worker;

namespace EventFoto.Processor.Functions;

public class ProcessImageFunction
{
    private readonly IImageProcessor _imageProcessor;
    private readonly IDownloadZipProcessor _downloadZipProcessor;

    public ProcessImageFunction(IImageProcessor imageProcessor,
        IDownloadZipProcessor downloadZipProcessor)
    {
        _imageProcessor = imageProcessor;
        _downloadZipProcessor = downloadZipProcessor;
    }

    [Function("ProcessMessage")]
    public async Task ProcessMessage(
        [QueueTrigger("%AzureStorage:ProcessingQueueName%", Connection = "AzureStorage:ConnectionString")] QueueMessage queueMessage,
        FunctionContext context, CancellationToken cancellationToken = default)

    {
        var message = JsonSerializer.Deserialize<ProcessingMessage>(queueMessage.MessageText);
        if (message == null) throw new InvalidOperationException("Invalid queue message");

        switch (message.Type)
        {
            case ProcessingMessageType.UploadBatch:
                await _imageProcessor.ProcessImagesAsync(message, cancellationToken);
                return;
            case ProcessingMessageType.DownloadZip:
                await _downloadZipProcessor.ProcessDownloadAsync(message, cancellationToken);
                return;
            default:
                throw new InvalidOperationException("Invalid message type");
        }
    }
}
