using EventFoto.Data.Models;

namespace EventFoto.Processor.ImageProcessor;

public interface IImageProcessor
{
    public Task ProcessImagesAsync(ProcessingMessage message, CancellationToken cancellationToken);
}
