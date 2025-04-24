using EventFoto.Data.Models;

namespace EventFoto.Data.Repositories;

public interface IUploadBatchRepository
{
    public Task<UploadBatch> GetByIdAsync(int id);
    public Task<UploadBatch> CreateAsync(UploadBatch uploadBatch);
    public Task MarkAsReadyAsync(int id);
    public Task DeleteBeforeDateAsync(DateTime date);
}
