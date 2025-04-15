using EventFoto.Data.Models;

namespace EventFoto.Processor.ImageProcessor;

public class ImageProcessor : IImageProcessor
{
    public async Task ProcessImageAsync(ProcessingMessage message, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Processing {message.Filename}");
    }
}
