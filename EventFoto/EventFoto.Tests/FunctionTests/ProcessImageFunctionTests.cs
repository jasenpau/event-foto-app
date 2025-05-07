using EventFoto.Data.Enums;
using EventFoto.Data.Models;
using EventFoto.Processor.Functions;
using EventFoto.Tests.TestBedSetup;
using FluentAssertions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;

namespace EventFoto.Tests.FunctionTests;

public class ProcessImageFunctionTests
{
    private readonly FunctionTestHost _host;
    private readonly ProcessImageFunction _function;
    private readonly FunctionContext _functionContext;

    public ProcessImageFunctionTests()
    {
        _host = new FunctionTestHost(services =>
        {
            services.AddTransient<ProcessImageFunction>();
        });
        _function = _host.GetService<ProcessImageFunction>();
        _functionContext = _host.GetService<FunctionContext>();
    }

    [Fact]
    public async Task ProcessImageFunction_InvalidMessageType()
    {
        // Arrange
        var processingMessage = new ProcessingMessage
        {
            Type = (ProcessingMessageType)20,
            EntityId = -1,
        };
        var queueMessage = QueueMessageMock.Serialize(processingMessage);

        // Act
        var act = async () => await _function.ProcessMessage(queueMessage, _functionContext);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invalid message type");
    }
}
