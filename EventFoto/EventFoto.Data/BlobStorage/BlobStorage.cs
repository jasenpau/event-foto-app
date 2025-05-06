using System.Net;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using EventFoto.Data.Extensions;
using EventFoto.Data.Models;
using Microsoft.Extensions.Configuration;

namespace EventFoto.Data.BlobStorage;

public class BlobStorage : IBlobStorage
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly IBlobServiceClientHelper _serviceClientHelper;
    private readonly IConfiguration _configuration;

    public BlobStorage(BlobServiceClient blobServiceClient,
        IBlobServiceClientHelper serviceClientHelper,
        IConfiguration configuration)
    {
        _blobServiceClient = blobServiceClient;
        _serviceClientHelper = serviceClientHelper;
        _configuration = configuration;
    }

    public string GetContainerName(int eventId) => $"event-{eventId}";

    public async Task CreateContainerAsync(string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();
    }

    public async Task<ServiceResult<string>> GetUploadSasUri(string containerName, int tokenExpiryInMinutes)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();

        if (!containerClient.CanGenerateSasUri)
            return ServiceResult<string>.Fail($"Cannot generate SAS URI for container '{containerName}'",
                HttpStatusCode.InternalServerError);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = containerName,
            Resource = "c",
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(tokenExpiryInMinutes)
        };
        sasBuilder.SetPermissions(BlobContainerSasPermissions.Write | BlobContainerSasPermissions.Create);

        var sasUri = containerClient.GenerateSasUri(sasBuilder);
        return ServiceResult<string>.Ok(sasUri.ToString());
    }

    public ServiceResult<string> GetReadOnlySasUri(int tokenExpiryInMinutes)
    {
        var accountKey = _configuration["AzureStorage:AccountKey"];
        var accountName = _serviceClientHelper.GetAccountName(_blobServiceClient);
        var credential = new StorageSharedKeyCredential(accountName, accountKey);

        var isDev = _configuration["IsDevelopment"] == "true";

        var sasBuilder = new AccountSasBuilder
        {
            Services = AccountSasServices.Blobs,
            ResourceTypes = AccountSasResourceTypes.Container | AccountSasResourceTypes.Object,
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(tokenExpiryInMinutes),
            Protocol = isDev ? SasProtocol.HttpsAndHttp : SasProtocol.Https
        };

        sasBuilder.SetPermissions(AccountSasPermissions.Read);

        var sasToken = sasBuilder.ToSasQueryParameters(credential).ToString();
        var serviceUri = _blobServiceClient.Uri;

        return ServiceResult<string>.Ok($"{serviceUri}?{sasToken}");
    }

    public async Task<ServiceResult<string>> UploadFileAsync(string containerName, string filename, Stream fileStream,
        string contentType, CancellationToken cancellationToken)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
            var blobClient = containerClient.GetBlobClient(filename);

            var httpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType
            };

            await blobClient.UploadAsync(fileStream, new BlobUploadOptions
            {
                HttpHeaders = httpHeaders
            }, cancellationToken: cancellationToken);

            return ServiceResult<string>.Ok(blobClient.Uri.ToString());
        }
        catch (Exception ex)
        {
            return ServiceResult<string>.Fail($"Upload failed: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public Task<ServiceResult<string>> UploadFileAsync(string containerName, string filename, Stream fileStream,
        CancellationToken cancellationToken)
    {
        return UploadFileAsync(containerName, filename, fileStream, "image/jpeg", cancellationToken);
    }

    public async Task<ServiceResult<MemoryStream>> DownloadFileAsync(string containerName, string filename,
        CancellationToken cancellationToken)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(filename);

        if (!await blobClient.ExistsAsync(cancellationToken))
        {
            return ServiceResult<MemoryStream>.Fail($"Blob '{filename}' not found in container '{containerName}'.",
                HttpStatusCode.NotFound);
        }

        var stream = new MemoryStream();
        await blobClient.DownloadToAsync(stream, cancellationToken);
        stream.Position = 0;
        return ServiceResult<MemoryStream>.Ok(stream);
    }

    public async Task<ServiceResult<int>> DeleteFilesAsync(string containerName, IList<string> filenames, CancellationToken cancellationToken)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

        var batchClient = _serviceClientHelper.CreateBatchClient(_blobServiceClient);
        var blobUris = filenames.Select(name => containerClient.GetBlobClient(name).Uri).ToList();

        var batches = blobUris.Batch(100);
        var successCount = 0;

        try
        {
            foreach (var batch in batches)
            {
                var response = await batchClient.DeleteBlobsAsync(
                    batch,
                    DeleteSnapshotsOption.IncludeSnapshots,
                    cancellationToken: cancellationToken
                );
                successCount += response.Count(x => !x.IsError);
            }

            return ServiceResult<int>.Ok(successCount);
        }
        catch (Exception ex)
        {
            return ServiceResult<int>.Fail($"Batch delete failed: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<ServiceResult<bool>> DeleteContainerAsync(string containerName, CancellationToken cancellationToken)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            if (!await containerClient.ExistsAsync(cancellationToken))
            {
                return ServiceResult<bool>.Fail($"Container '{containerName}' not found.", HttpStatusCode.NotFound);
            }

            await containerClient.DeleteAsync(conditions: null, cancellationToken);
            return ServiceResult<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.Fail($"Container deletion failed: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }
}
