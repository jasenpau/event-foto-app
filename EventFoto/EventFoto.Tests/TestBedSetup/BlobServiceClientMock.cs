using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using EventFoto.Data.BlobStorage;
using Moq;

namespace EventFoto.Tests.TestBedSetup;

public static class BlobServiceClientMock
{
    public static Mock<BlobServiceClient> GetMock()
    {
        var testUri = new Uri("https://account_name.exmaple.com");
        var blobServiceClientMock = new Mock<BlobServiceClient>(testUri, null!);
        var blobContainerClientMock = new Mock<BlobContainerClient>();
        var blobClientMock = new Mock<BlobClient>(testUri, null!);

        blobServiceClientMock.Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
            .Returns(blobContainerClientMock.Object);

        blobContainerClientMock.Setup(x => x.GetBlobClient(It.IsAny<string>()))
            .Returns(blobClientMock.Object);
        blobContainerClientMock.Setup(x => x.CanGenerateSasUri).Returns(true);
        blobContainerClientMock.Setup(x => x.GenerateSasUri(It.IsAny<BlobSasBuilder>()))
            .Returns(testUri);

        blobClientMock.SetupGet(x => x.Uri).Returns(testUri);
        var response = new Mock<Response<bool>>();
        response.SetupGet(x => x.Value).Returns(true);
        blobClientMock.Setup(x => x.ExistsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(response.Object);
        blobClientMock.Setup(x => x.DownloadToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>())).Callback(
            (Stream stream, CancellationToken _) =>
            {
                using var fileStream = File.OpenRead("TestFiles/test.jpg");
                fileStream.CopyTo(stream);
            });

        return blobServiceClientMock;
    }

    public static Mock<IBlobServiceClientHelper> GetBlobHelperMock()
    {
        var blobServiceClientHelperMock = new Mock<IBlobServiceClientHelper>();
        var batchClientMock = new Mock<BlobBatchClient>();

        blobServiceClientHelperMock.Setup(x => x.CreateBatchClient(It.IsAny<BlobServiceClient>()))
            .Returns(batchClientMock.Object);
        blobServiceClientHelperMock.Setup(x => x.GetAccountName(It.IsAny<BlobServiceClient>()))
            .Returns("TestAccountName");

        return blobServiceClientHelperMock;
    }
}
