using EventFoto.Data.Models;

namespace EventFoto.Data.PhotoStorage;

public interface IPhotoBlobStorage
{
    public string GetContainerName(int eventId);
    public Task CreateContainerAsync(string containerName);
    public Task<ServiceResult<string>> GetUploadSasUri(string containerName, int tokenExpiryInMinutes);

    public Task<ServiceResult<MemoryStream>> DownloadImageAsync(string containerName, string filename,
        CancellationToken cancellationToken);
}
