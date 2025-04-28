using EventFoto.Data.Models;

namespace EventFoto.Data.Repositories;

public interface IPhotographerAssignmentRepository
{
    public Task<PhotographerAssignment> GetForEventAsync(int eventId, Guid userId);
    public Task<List<PhotographerAssignment>> GetAssignmentsForEventAsync(int eventId);
    public Task<bool> AssignPhotographerAsync(PhotographerAssignment assignment);
    public Task<bool> RemoveAssignmentAsync(PhotographerAssignment assignment);
}
