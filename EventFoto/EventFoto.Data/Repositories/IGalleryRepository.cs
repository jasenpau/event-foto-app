using EventFoto.Data.DatabaseProjections;
using EventFoto.Data.DTOs;
using EventFoto.Data.Models;

namespace EventFoto.Data.Repositories;

public interface IGalleryRepository
{
    public Task<Gallery> GetByIdAsync(int id);
    public Task<bool> NameExistsAsync(string name, int eventId);
    public Task<Gallery> CreateAsync(Gallery gallery);
    public Task<Gallery> UpdateAsync(Gallery gallery);
    public Task<bool> DeleteAsync(int id);
    public Task<List<EventGalleryProjection>> GetByEventIdAsync(int eventId);
}
