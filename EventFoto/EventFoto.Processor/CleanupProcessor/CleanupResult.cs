namespace EventFoto.Processor.CleanupProcessor;

public record CleanupResult
{
    public int DownloadRequests { get; init; } = 0;
    public int DownloadZips { get; init; } = 0;
    public int UploadBatches { get; init; } = 0;
}
