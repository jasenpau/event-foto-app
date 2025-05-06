using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;

namespace EventFoto.Data.BlobStorage;

public class BlobServiceClientHelper : IBlobServiceClientHelper
{
    public BlobBatchClient CreateBatchClient(BlobServiceClient client) => client.GetBlobBatchClient();
    public string GetAccountName(BlobServiceClient client) => client.AccountName;
}
