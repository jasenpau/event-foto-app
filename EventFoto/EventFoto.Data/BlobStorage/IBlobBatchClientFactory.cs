using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;

namespace EventFoto.Data.BlobStorage;

public interface IBlobBatchClientFactory
{
    public BlobBatchClient Create(BlobServiceClient client);
}
