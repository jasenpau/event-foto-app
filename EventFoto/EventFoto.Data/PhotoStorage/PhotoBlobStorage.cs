using System.Net;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using EventFoto.Data.Extensions;
using EventFoto.Data.Models;
using Microsoft.Extensions.Configuration;

namespace EventFoto.Data.PhotoStorage;

public class PhotoBlobStorage : IPhotoBlobStorage
{
    private readonly string _connectionString;

    public PhotoBlobStorage(IConfiguration configuration)
    {
        _connectionString = configuration["AzureStorage:ConnectionString"];
    }

    public string GetContainerName(int eventId) => $"event-{eventId}";

    public async Task CreateContainerAsync(string containerName)
    {
        var blobClient = new BlobServiceClient(_connectionString);
        var containerClient = blobClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();
    }

    public async Task<ServiceResult<string>> GetUploadSasUri(string containerName, int tokenExpiryInMinutes)
    {
        var containerClient = new BlobContainerClient(_connectionString, containerName);
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

    public async Task<ServiceResult<MemoryStream>> DownloadImageAsync(string containerName, string filename,
        CancellationToken cancellationToken)
    {
        var containerClient = new BlobContainerClient(_connectionString, containerName);
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

    public async Task<ServiceResult<int>> DeleteImagesAsync(string containerName, IList<string> filenames, CancellationToken cancellationToken)
    {
        var blobClient = new BlobServiceClient(_connectionString);
        var containerClient = blobClient.GetBlobContainerClient(containerName);

        var batchClient = blobClient.GetBlobBatchClient();
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
}
