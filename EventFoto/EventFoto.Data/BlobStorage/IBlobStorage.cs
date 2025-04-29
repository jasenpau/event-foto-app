using EventFoto.Data.Models;

namespace EventFoto.Data.BlobStorage;

public interface IBlobStorage
{
    public string GetContainerName(int eventId);
    public Task CreateContainerAsync(string containerName);
    public Task<ServiceResult<string>> GetUploadSasUri(string containerName, int tokenExpiryInMinutes);
    public ServiceResult<string> GetReadOnlySasUri(int tokenExpiryInMinutes);
    public Task<ServiceResult<string>> UploadFileAsync(string containerName, string filename, Stream fileStream,
        CancellationToken cancellationToken);
    public Task<ServiceResult<string>> UploadFileAsync(string containerName, string filename, Stream fileStream,
        string contentType, CancellationToken cancellationToken);
    public Task<ServiceResult<MemoryStream>> DownloadFileAsync(string containerName, string filename,
        CancellationToken cancellationToken);
    public Task<ServiceResult<int>> DeleteFilesAsync(string containerName, IList<string> filenames,
        CancellationToken cancellationToken);
    public Task<ServiceResult<bool>> DeleteContainerAsync(string containerName, CancellationToken cancellationToken);
}
