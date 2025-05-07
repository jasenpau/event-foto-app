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

public class ProcessImageArchiveEventTests
{
    private readonly FunctionTestHost _host;
    private readonly ProcessImageFunction _function;
    private readonly FunctionContext _functionContext;
    private readonly EventFotoContext _context;

    public ProcessImageArchiveEventTests()
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
    public async Task ProcessImageFunction_ArchiveEvent_ValidMessage()
    {
        // Arrange
        await _context.Reset();
        await _context.AddUser(UserConstants.GetTestPhotographer());
        await _context.AddUser(UserConstants.GetTestEventAdmin());
        await _context.AddEvent(EventConstants.GetCurrentEvent());
        await _context.AddPhotos(PhotoConstants.GetTestPhotos(1));

        var message = new ProcessingMessage
        {
            Type = ProcessingMessageType.ArchiveEvent,
            EntityId = 1,
            Filename = "archive.zip",
            EnqueuedOn = DateTime.UtcNow
        };
        var queueMessage = QueueMessageMock.Serialize(message);

        // Act
        await _function.ProcessMessage(queueMessage, _functionContext);

        // Assert
        var eventResult = _context.Events.First(x => x.Id == 1);
        eventResult.Should().NotBeNull();
        eventResult.IsArchived.Should().BeTrue();
        _context.EventPhotos.Count().Should().Be(0);
    }
}
