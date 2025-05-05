using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using EventFoto.Data.BlobStorage;
using Moq;

namespace EventFoto.Tests.TestBedSetup;

public static class BlobServiceClientMock
{
    public static Mock<BlobServiceClient> GetMock()
    {
        var blobServiceClientMock = new Mock<BlobServiceClient>();
        var blobContainerClientMock = new Mock<BlobContainerClient>();
        var blobClientMock = new Mock<BlobClient>();

        blobServiceClientMock.Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
            .Returns(blobContainerClientMock.Object);

        blobContainerClientMock.Setup(x => x.GetBlobClient(It.IsAny<string>()))
            .Returns(blobClientMock.Object);

        return blobServiceClientMock;
    }

    public static Mock<IBlobBatchClientFactory> GetBlobBatchClientFactoryMock()
    {
        var blobBatchClientFactoryMock = new Mock<IBlobBatchClientFactory>();
        var batchClientMock = new Mock<BlobBatchClient>();

        blobBatchClientFactoryMock.Setup(x => x.Create(It.IsAny<BlobServiceClient>()))
            .Returns(batchClientMock.Object);

        return blobBatchClientFactoryMock;
    }
}
