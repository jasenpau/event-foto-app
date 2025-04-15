using EventFoto.Data.Models;

namespace EventFoto.Processor.ImageProcessor;

public interface IImageProcessor
{
    public Task ProcessImageAsync(ProcessingMessage message, CancellationToken cancellationToken);
}
