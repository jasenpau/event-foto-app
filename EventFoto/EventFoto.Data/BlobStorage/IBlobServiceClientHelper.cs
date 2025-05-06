using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;

namespace EventFoto.Data.BlobStorage;

public interface IBlobServiceClientHelper
{
    public BlobBatchClient CreateBatchClient(BlobServiceClient client);
    public string GetAccountName(BlobServiceClient client);
}
