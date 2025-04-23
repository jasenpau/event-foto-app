using System.Globalization;
using EventFoto.Data.DTOs;
using EventFoto.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EventFoto.Data.Repositories;

public class EventPhotoRepository : IEventPhotoRepository
{
    private readonly EventFotoContext _context;
    private DbSet<EventPhoto> EventPhotos => _context.EventPhotos;

    public EventPhotoRepository(EventFotoContext context)
    {
        _context = context;
    }

    public Task<EventPhoto> GetByIdAsync(int photoId)
    {
        return EventPhotos
            .Include(p => p.User)
            .Include(p => p.Gallery).ThenInclude(g => g.Event)
            .FirstOrDefaultAsync(p => p.Id == photoId);
    }

    public async Task<EventPhoto> AddEventPhotoAsync(EventPhoto eventPhoto)
    {
        EventPhotos.Add(eventPhoto);
        await _context.SaveChangesAsync();
        return eventPhoto;
    }

    public async Task<EventPhoto> MarkAsProcessed(EventPhoto eventPhoto, string processedFilename)
    {
        eventPhoto.IsProcessed = true;
        eventPhoto.ProcessedFilename = processedFilename;
        _context.Entry(eventPhoto).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return eventPhoto;
    }

    public Task<EventPhoto> GetByEventAndFilename(int eventId, string filename)
    {
        return EventPhotos
            .Include(p => p.User)
            .Include(p => p.Gallery).ThenInclude(g => g.Event)
            .Where(p => p.Gallery.EventId == eventId && p.Filename == filename)
            .SingleOrDefaultAsync();
    }

    public async Task<PagedData<string, EventPhoto>> SearchPhotosAsync(EventPhotoSearchParams searchParams)
    {
        IQueryable<EventPhoto> query = EventPhotos.Where(p => p.IsProcessed);

        if (searchParams.GalleryId.HasValue)
        {
            query = query.Where(p => p.GalleryId == searchParams.GalleryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchParams.KeyOffset))
        {
            var offset = searchParams.KeyOffset.Split('|');
            var offsetDate = DateTime.Parse(offset[0], CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
            var offsetId = int.Parse(offset[1]);

            query = query.Where(p =>
                p.CaptureDate > offsetDate ||
                (p.CaptureDate == offsetDate && p.Id > offsetId));
        }

        if (searchParams.FromDate.HasValue)
        {
            query = query.Where(p => p.CaptureDate >= searchParams.FromDate.Value);
        }

        if (searchParams.ToDate.HasValue)
        {
            query = query.Where(p => p.CaptureDate <= searchParams.ToDate.Value);
        }

        query = query.OrderBy(p => p.CaptureDate).ThenBy(p => p.Id);
        query = query.Take(searchParams.PageSize + 1);

        var queryResult = await query.ToListAsync();
        var hasNextPage = queryResult.Count > searchParams.PageSize;
        if (hasNextPage) queryResult.RemoveAt(queryResult.Count - 1);

        return new PagedData<string, EventPhoto>
        {
            Data = queryResult,
            PageSize = searchParams.PageSize,
            KeyOffset = searchParams.KeyOffset,
            HasNextPage = hasNextPage
        };
    }

    public Task<List<EventPhoto>> GetByIdsAsync(IList<int> photoIds)
    {
        return EventPhotos
            .Include(p => p.Gallery)
            .Where(p => photoIds.Contains(p.Id))
            .ToListAsync();
    }

    public async Task DeleteEventPhotosAsync(IList<EventPhoto> photos, CancellationToken cancellationToken)
    {
        EventPhotos.RemoveRange(photos);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task UpdateEventPhotosAsync(IList<EventPhoto> photos)
    {
        EventPhotos.UpdateRange(photos);
        return _context.SaveChangesAsync();
    }

    public Task MoveAllGalleryPhotosAsync(int sourceGalleryId, int destinationGalleryId)
    {
        return EventPhotos
            .Where(p => p.GalleryId == sourceGalleryId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(p => p.GalleryId, destinationGalleryId));
    }
}
