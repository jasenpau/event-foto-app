using EventFoto.Data.Models;

namespace EventFoto.Processor.DownloadZipProcessor;

public interface IDownloadZipProcessor
{
    public Task<int> ProcessDownloadAsync(ProcessingMessage message, CancellationToken cancellationToken);
}
