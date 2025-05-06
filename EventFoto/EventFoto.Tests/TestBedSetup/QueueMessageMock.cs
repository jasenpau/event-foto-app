using Azure.Storage.Queues.Models;
using EventFoto.Data.Models;
using Moq;

namespace EventFoto.Tests.TestBedSetup;

public static class QueueMessageMock
{
    public static QueueMessage Serialize(ProcessingMessage message)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(message);
        var body = BinaryData.FromString(json);

        var queueMessage = QueuesModelFactory.QueueMessage(
            messageId: "1",
            popReceipt: "2",
            body: body,
            dequeueCount: 1,
            insertedOn: DateTimeOffset.UtcNow
        );

        return queueMessage;
    }
}
