using EventFoto.Data.Models;

namespace EventFoto.Processor.ImageProcessor;

public interface IImageProcessor
{
    public Task<int> ProcessImagesAsync(ProcessingMessage message, CancellationToken cancellationToken);
    public Task<int> ReprocessEventImages(ProcessingMessage message, CancellationToken cancellationToken = default);
    public Task<int> ReprocessGalleryImages(ProcessingMessage message, CancellationToken cancellationToken = default);
}
