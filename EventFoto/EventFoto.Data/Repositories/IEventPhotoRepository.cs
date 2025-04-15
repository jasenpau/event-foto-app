using EventFoto.Data.Models;

namespace EventFoto.Data.Repositories;

public interface IEventPhotoRepository
{
    public Task<EventPhoto> AddEventPhotoAsync(EventPhoto eventPhoto);
}
