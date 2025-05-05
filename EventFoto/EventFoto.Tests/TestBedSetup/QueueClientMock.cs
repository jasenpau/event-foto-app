using Azure;
using Azure.Storage.Queues;
using EventFoto.Core.Processing;
using Moq;

namespace EventFoto.Tests.TestBedSetup;

public static class QueueClientMock
{
    public static Mock<IQueueClientFactory> GetFactoryMock()
    {
        var queueClientMock = new Mock<QueueClient>();
        var queueClientFactoryMock = new Mock<IQueueClientFactory>();
        var existsResponseMock = new Mock<Response<bool>>();
        existsResponseMock.Setup(x => x.Value).Returns(true);

        queueClientMock
            .Setup(x => x.CreateIfNotExistsAsync(It.IsAny<IDictionary<string, string>?>(), It.IsAny<CancellationToken>()));
        queueClientMock
            .Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existsResponseMock.Object);

        queueClientFactoryMock.Setup(x => x.GetClient()).Returns(queueClientMock.Object);

        return queueClientFactoryMock;
    }
}
