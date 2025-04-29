using EventFoto.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EventFoto.Data.Repositories;

public class DownloadRequestRepository : IDownloadRequestRepository
{
    private readonly EventFotoContext _context;
    private DbSet<DownloadRequest> DownloadRequests => _context.DownloadRequests;

    public DownloadRequestRepository(EventFotoContext context)
    {
        _context = context;
    }

    public Task<DownloadRequest> GetByIdAsync(int id)
    {
        return DownloadRequests.FirstOrDefaultAsync(e => e.Id == id);
    }

    public Task<DownloadRequest> GetWithImagesAsync(int id)
    {
        return DownloadRequests
            .Include(e => e.DownloadImages)
            .ThenInclude(e => e.EventPhoto)
            .ThenInclude(e => e.Gallery)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<DownloadRequest> CreateAsync(DownloadRequest downloadRequest)
    {
        downloadRequest.IsReady = false;
        await DownloadRequests.AddAsync(downloadRequest);
        await _context.SaveChangesAsync();
        return downloadRequest;
    }

    public Task<List<DownloadRequest>> GetBeforeDate(DateTime date)
    {
        return DownloadRequests
            .Where(e => e.ProcessedOn.HasValue && e.ProcessedOn <= date)
            .ToListAsync();
    }

    public Task DeleteAsync(IList<DownloadRequest> downloadRequests)
    {
        DownloadRequests.RemoveRange(downloadRequests);
        return _context.SaveChangesAsync();
    }

    public async Task MarkAsReady(int id)
    {
        var entity = await DownloadRequests.FirstOrDefaultAsync(e => e.Id == id);
        if (entity == null)
            throw new KeyNotFoundException($"DownloadRequest with Id {id} not found.");

        entity.IsReady = true;
        entity.ProcessedOn = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
