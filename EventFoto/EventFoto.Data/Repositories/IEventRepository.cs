using EventFoto.Data.Models;

namespace EventFoto.Data.Repositories;

public interface IEventRepository
{
    public Task<Event> GetByIdAsync(int id);
    public Task<IList<Event>> GetAllEventsByUserAsync(Guid userId);
    public Task<Event> CreateAsync(Event eventData);
}
