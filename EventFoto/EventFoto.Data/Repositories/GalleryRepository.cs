using EventFoto.Data.DatabaseProjections;
using EventFoto.Data.DTOs;
using EventFoto.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EventFoto.Data.Repositories;

public class GalleryRepository : IGalleryRepository
{
    private readonly EventFotoContext _context;
    private DbSet<Gallery> Galleries => _context.Galleries;

    public GalleryRepository(EventFotoContext context)
    {
        _context = context;
    }

    public async Task<Gallery> GetByIdAsync(int id)
    {
        return await Galleries
            .Include(g => g.Event)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<bool> NameExistsAsync(string name, int eventId)
    {
        var existingGallery = await Galleries.FirstOrDefaultAsync(g => g.Name == name && g.EventId == eventId);
        return existingGallery != null;
    }

    public async Task<Gallery> CreateAsync(Gallery gallery)
    {
        Galleries.Add(gallery);
        await _context.SaveChangesAsync();
        return gallery;
    }

    public async Task<Gallery> UpdateAsync(Gallery gallery)
    {
        _context.Update(gallery);
        await _context.SaveChangesAsync();
        return gallery;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var gallery = await Galleries.FindAsync(id);
        if (gallery == null)
        {
            return false;
        }

        Galleries.Remove(gallery);
        await _context.SaveChangesAsync();
        return true;
    }

    public Task<List<EventGalleryProjection>> GetPagedByEventIdAsync(int eventId)
    {
        return _context.Database
            .SqlQuery<EventGalleryProjection>($"SELECT * FROM get_event_galleries({eventId})")
            .ToListAsync();
    }
}
