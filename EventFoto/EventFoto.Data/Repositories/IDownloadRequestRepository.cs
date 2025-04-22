using EventFoto.Data.Models;

namespace EventFoto.Data.Repositories;

public interface IDownloadRequestRepository
{
    public Task<DownloadRequest> GetByIdAsync(int id);
    public Task<DownloadRequest> GetWithImagesAsync(int id);
    public Task<DownloadRequest> CreateAsync(DownloadRequest downloadRequest);
    public Task<List<DownloadRequest>> GetBeforeDate(DateTime date);
    public Task DeleteAsync(IList<DownloadRequest> downloadRequests);
    public Task MarkAsReady(int id);
}
