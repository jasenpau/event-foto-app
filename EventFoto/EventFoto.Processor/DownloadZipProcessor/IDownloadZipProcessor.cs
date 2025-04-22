using EventFoto.Data.Models;

namespace EventFoto.Processor.DownloadZipProcessor;

public interface IDownloadZipProcessor
{
    public Task ProcessDownloadAsync(ProcessingMessage message, CancellationToken cancellationToken);
}
