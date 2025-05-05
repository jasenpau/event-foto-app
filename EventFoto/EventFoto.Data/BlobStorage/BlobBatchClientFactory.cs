using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;

namespace EventFoto.Data.BlobStorage;

public class BlobBatchClientFactory : IBlobBatchClientFactory
{
    public BlobBatchClient Create(BlobServiceClient client) => client.GetBlobBatchClient();
}
