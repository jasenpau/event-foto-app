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

    public async Task<EventPhoto> AddEventPhotoAsync(EventPhoto eventPhoto)
    {
        EventPhotos.Add(eventPhoto);
        await _context.SaveChangesAsync();
        return eventPhoto;
    }
}
