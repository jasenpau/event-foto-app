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

public class ProcessImageReprocessEventTests
{
    private readonly FunctionTestHost _host;
    private readonly ProcessImageFunction _function;
    private readonly FunctionContext _functionContext;
    private readonly EventFotoContext _context;

    public ProcessImageReprocessEventTests()
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
        gallery.WatermarkId = 1;
        _context.Galleries.Update(gallery);
        await _context.SaveChangesAsync();
        await _context.AddPhotos(PhotoConstants.GetTestPhotos(1));

        var message = new ProcessingMessage
        {
            Type = ProcessingMessageType.ReprocessEvent,
            EntityId = 1,
            EnqueuedOn = DateTime.UtcNow
        };
        var queueMessage = QueueMessageMock.Serialize(message);

        // Act
        await _function.ProcessMessage(queueMessage, _functionContext);

        // Assert
        var photos = _context.EventPhotos.Where(x => x.IsProcessed == true).ToList();
        photos.Should().NotBeEmpty();
        photos.ForEach(x =>
        {
            x.WatermarkId.Should().Be(1);
        });
    }
}
