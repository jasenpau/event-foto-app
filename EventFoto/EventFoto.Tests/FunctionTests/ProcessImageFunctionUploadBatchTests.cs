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

public class ProcessImageFunctionUploadBatchTests
{
    private readonly FunctionTestHost _host;
    private readonly ProcessImageFunction _function;
    private readonly FunctionContext _functionContext;
    private readonly EventFotoContext _context;

    public ProcessImageFunctionUploadBatchTests()
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
    public async Task ProcessImageFunction_UploadBatch_ValidMessage()
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
        var uploadBatch = PhotoConstants.GetTestUploadBatch();
        await _context.AddUploadBatch(uploadBatch);

        var message = new ProcessingMessage
        {
            Type = ProcessingMessageType.UploadBatch,
            EntityId = 1,
            Filename = "test.jpg",
            EnqueuedOn = DateTime.UtcNow
        };
        var queueMessage = QueueMessageMock.Serialize(message);

        // Act
        await _function.ProcessMessage(queueMessage, _functionContext);

        // Assert
        var photo = _context.EventPhotos.First(x => x.Id == 3);
        photo.Should().NotBeNull();
        photo.IsProcessed.Should().BeTrue();
        photo.ProcessedFilename.Should().Be("out-test3.jpg");
        var processedBatch = _context.UploadBatches.First(x => x.Id == 1);
        processedBatch.IsReady.Should().BeTrue();
        processedBatch.ProcessedOn.Should().NotBeNull();
    }

    [Fact]
    public async Task ProcessImageFunction_UploadBatch_NonExistentBatch()
    {
        // Arrange
        var message = new ProcessingMessage
        {
            Type = ProcessingMessageType.UploadBatch,
            EntityId = 1,
            Filename = "test.jpg",
            EnqueuedOn = DateTime.UtcNow
        };
        var queueMessage = QueueMessageMock.Serialize(message);

        // Act
        var act = async () => await _function.ProcessMessage(queueMessage, _functionContext);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Upload batch not found");
    }
}
