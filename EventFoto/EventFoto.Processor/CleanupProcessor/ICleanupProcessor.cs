namespace EventFoto.Processor.CleanupProcessor;

public interface ICleanupProcessor
{
    public Task<CleanupResult> CleanupAsync(DateTime executionDateTime, CancellationToken cancellationToken);
}
