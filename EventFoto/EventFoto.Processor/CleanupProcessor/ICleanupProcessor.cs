namespace EventFoto.Processor.CleanupProcessor;

public interface ICleanupProcessor
{
    public Task<int> CleanupAsync(DateTime executionDateTime, CancellationToken cancellationToken);
}
