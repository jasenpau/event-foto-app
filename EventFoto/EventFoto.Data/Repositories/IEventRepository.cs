using EventFoto.Data.Models;

namespace EventFoto.Data.Repositories;

public interface IEventRepository
{
    public Task<IList<Event>> GetAllEventsByUser(int userId);
    public Task<Event> CreateAsync(Event eventData);
}
