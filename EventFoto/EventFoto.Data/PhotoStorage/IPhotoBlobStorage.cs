using EventFoto.Data.Models;

namespace EventFoto.Data.PhotoStorage;

public interface IPhotoBlobStorage
{
    public string GetContainerName(int eventId);
    public Task CreateContainerAsync(string containerName);
    public Task<ServiceResult<string>> GetUploadSasUri(string containerName, int tokenExpiryInMinutes);
    public ServiceResult<string> GetReadOnlySasUri(int tokenExpiryInMinutes);
    public Task<ServiceResult<string>> UploadImageAsync(string containerName, string filename, Stream imageStream,
        CancellationToken cancellationToken);
    public Task<ServiceResult<MemoryStream>> DownloadImageAsync(string containerName, string filename,
        CancellationToken cancellationToken);
    public Task<ServiceResult<int>> DeleteImagesAsync(string containerName, IList<string> filenames,
        CancellationToken cancellationToken);
}
