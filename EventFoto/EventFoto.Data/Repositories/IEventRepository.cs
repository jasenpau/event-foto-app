using EventFoto.Data.Models;

namespace EventFoto.Data.Repositories;

public interface IEventRepository
{
    public Task<IList<Event>> GetAllEventsByUser(Guid userId);
    public Task<Event> CreateAsync(Event eventData);
}
