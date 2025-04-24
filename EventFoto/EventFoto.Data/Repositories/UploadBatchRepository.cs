using EventFoto.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EventFoto.Data.Repositories;

public class UploadBatchRepository : IUploadBatchRepository
{
    private readonly EventFotoContext _context;
    private DbSet<UploadBatch> UploadBatches => _context.UploadBatches;

    public UploadBatchRepository(EventFotoContext context)
    {
        _context = context;
    }

    public Task<UploadBatch> GetByIdAsync(int id)
    {
        return UploadBatches
            .Include(e => e.EventPhotos)
            .ThenInclude(e => e.Gallery)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<UploadBatch> CreateAsync(UploadBatch uploadBatch)
    {
        await UploadBatches.AddAsync(uploadBatch);
        await _context.SaveChangesAsync();
        return uploadBatch;
    }

    public async Task MarkAsReadyAsync(int id)
    {
        var batch = await UploadBatches.FirstOrDefaultAsync(x => x.Id == id);
        batch.IsReady = true;
        batch.ProcessedOn = DateTime.UtcNow;
        _context.Update(batch);
        await _context.SaveChangesAsync();
    }

    public Task DeleteBeforeDateAsync(DateTime date)
    {
        var batches = UploadBatches.Where(x => x.ProcessedOn < date);
        UploadBatches.RemoveRange(batches);
        return _context.SaveChangesAsync();
    }
}
