using EventFoto.Data;
using EventFoto.Data.Enums;
using EventFoto.Data.Models;
using EventFoto.Processor.Functions;
using EventFoto.Tests.TestBedSetup;
using EventFoto.Tests.TestConstants;
using FluentAssertions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;

namespace EventFoto.Tests.FunctionTests;

public class ProcessImageDownloadZipTests
{
    private readonly FunctionTestHost _host;
    private readonly ProcessImageFunction _function;
    private readonly FunctionContext _functionContext;
    private readonly EventFotoContext _context;

    public ProcessImageDownloadZipTests()
    {
        _host = new FunctionTestHost(services =>
        {
            services.AddTransient<ProcessImageFunction>();
        });
        _function = _host.GetService<ProcessImageFunction>();
        _functionContext = _host.GetService<FunctionContext>();
        _context = _host.GetDbContext();
    }

    [Fact]
    public async Task ProcessImageFunction_DownloadZip_ValidMessage()
    {
        // Arrange
        await _context.Reset();
        await _context.AddUser(UserConstants.GetTestPhotographer());
        await _context.AddUser(UserConstants.GetTestEventAdmin());
        await _context.AddEvent(EventConstants.GetCurrentEvent());
        await _context.AddPhotos(PhotoConstants.GetTestPhotos(1));
        var downloadRequest = PhotoConstants.GetTestDownloadRequest();
        await _context.AddDownloadRequest(downloadRequest);

        var message = new ProcessingMessage
        {
            Type = ProcessingMessageType.DownloadZip,
            EntityId = downloadRequest.Id,
            Filename = "test.zip",
            EnqueuedOn = DateTime.UtcNow
        };
        var queueMessage = QueueMessageMock.Serialize(message);

        // Act
        await _function.ProcessMessage(queueMessage, _functionContext);

        // Assert
        var downloadRequestResult = _context.DownloadRequests.First(x => x.Id == downloadRequest.Id);
        downloadRequestResult.Should().NotBeNull();
        downloadRequestResult.IsReady.Should().BeTrue();
        downloadRequestResult.ProcessedOn.Should().NotBeNull();
    }

    [Fact]
    public async Task ProcessImageFunction_DownloadZip_WithQuality()
    {
        // Arrange
        await _context.Reset();
        await _context.AddUser(UserConstants.GetTestPhotographer());
        await _context.AddUser(UserConstants.GetTestEventAdmin());
        await _context.AddEvent(EventConstants.GetCurrentEvent());
        await _context.AddPhotos(PhotoConstants.GetTestPhotos(1));
        var downloadRequest = PhotoConstants.GetTestDownloadRequest();
        downloadRequest.Quality = 500;
        await _context.AddDownloadRequest(downloadRequest);

        var message = new ProcessingMessage
        {
            Type = ProcessingMessageType.DownloadZip,
            EntityId = downloadRequest.Id,
            Filename = "test.zip",
            EnqueuedOn = DateTime.UtcNow
        };
        var queueMessage = QueueMessageMock.Serialize(message);

        // Act
        await _function.ProcessMessage(queueMessage, _functionContext);

        // Assert
        var downloadRequestResult = _context.DownloadRequests.First(x => x.Id == downloadRequest.Id);
        downloadRequestResult.Should().NotBeNull();
        downloadRequestResult.IsReady.Should().BeTrue();
        downloadRequestResult.ProcessedOn.Should().NotBeNull();
    }

    [Fact]
    public async Task ProcessImageFunction_DownloadZip_NonExistentRequest()
    {
        // Arrange
        var message = new ProcessingMessage
        {
            Type = ProcessingMessageType.DownloadZip,
            EntityId = 1123,
            Filename = "test.zip",
            EnqueuedOn = DateTime.UtcNow
        };
        var queueMessage = QueueMessageMock.Serialize(message);

        // Act
        var act = async () => await _function.ProcessMessage(queueMessage, _functionContext);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Download request not found");
    }
}
