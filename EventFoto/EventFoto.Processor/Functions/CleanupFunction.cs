using EventFoto.Processor.CleanupProcessor;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace EventFoto.Processor.Functions;

public class CleanupFunction
{
    private readonly ICleanupProcessor _cleanupProcessor;
    private readonly ILogger _logger;

    public CleanupFunction(ICleanupProcessor cleanupProcessor,
        ILogger<CleanupFunction> logger)
    {
        _cleanupProcessor = cleanupProcessor;
        _logger = logger;
    }

    [Function("Cleanup")]
    public async Task RunAsync([TimerTrigger("%ProcessorOptions:CleanupSchedule%")] TimerInfo myTimer,
        FunctionContext context, CancellationToken cancellationToken)
    {
        var executionDateTime = DateTime.UtcNow;
        var result = await _cleanupProcessor.CleanupAsync(executionDateTime, cancellationToken);
        _logger.Log(LogLevel.Information,
            "Clean up processor executed at: {time}. " +
            "Deleted {downloadReq} download requests, " +
            "{downloadZips} download zips, " +
            "{uploadBatches} upload batches.",
            executionDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
            result.DownloadRequests,
            result.DownloadZips,
            result.UploadBatches);
    }
}
