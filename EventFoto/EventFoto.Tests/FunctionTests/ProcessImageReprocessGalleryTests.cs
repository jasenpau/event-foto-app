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

public class ProcessImageReprocessGalleryTests
{
    private readonly FunctionTestHost _host;
    private readonly ProcessImageFunction _function;
    private readonly FunctionContext _functionContext;
    private readonly EventFotoContext _context;

    public ProcessImageReprocessGalleryTests()
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
    public async Task ProcessImageFunction_ReprocessEvent_ValidMessage()
    {
        // Arrange
        await _context.Reset();
        await _context.AddUser(UserConstants.GetTestPhotographer());
        await _context.AddUser(UserConstants.GetTestEventAdmin());
        await _context.AddEvent(EventConstants.GetCurrentEvent());
        var gallery = _context.Galleries.First();
        var newGallery = GalleryConstants.GetTestGallery(2);
        await _context.AddGallery(newGallery);
        await _context.AddPhotos(PhotoConstants.GetTestPhotos(newGallery.Id));
        await _context.AddPhoto(new EventPhoto()
        {
            Filename = "test-10.jpg",
            ProcessedFilename = "out-test-10.jpg",
            GalleryId = gallery.Id,
            CaptureDate = DateTime.UtcNow.AddMinutes(-6),
            UploadDate = DateTime.UtcNow.AddMinutes(-6),
            IsProcessed = true,
        });

        var message = new ProcessingMessage
        {
            Type = ProcessingMessageType.ReprocessGallery,
            EntityId = newGallery.Id,
            EnqueuedOn = DateTime.UtcNow
        };
        var queueMessage = QueueMessageMock.Serialize(message);

        // Act
        await _function.ProcessMessage(queueMessage, _functionContext);

        // Assert
        var reprocessedPhotos = _context.EventPhotos
            .Where(x => x.IsProcessed == true && x.GalleryId == newGallery.Id)
            .ToList();
        reprocessedPhotos.Should().NotBeEmpty();
        reprocessedPhotos.ForEach(x =>
        {
            x.WatermarkId.Should().Be(1);
        });
        var mainGalleryPhotos = _context.EventPhotos
            .Where(x => x.IsProcessed == true && x.GalleryId == gallery.Id)
            .ToList();
        mainGalleryPhotos.Should().NotBeEmpty();
        mainGalleryPhotos.ForEach(x =>
        {
            x.WatermarkId.Should().BeNull();
        });
    }
}
