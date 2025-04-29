using EventFoto.Data.Models;

namespace EventFoto.Processor.EventArchiveProcessor;

public interface IEventArchiveProcessor
{
    public Task<int> ArchiveEventAsync(ProcessingMessage message, CancellationToken cancellationToken);
}
