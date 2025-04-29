using EventFoto.Data.Models;

namespace EventFoto.Core.Assignments;

public interface IAssignmentService
{
    public Task<ServiceResult<PhotographerAssignment>> GetUserAssignmentAsync(int eventId, Guid userId);
    public Task<ServiceResult<bool>> AssignPhotographerAsync(int galleryId, Guid userId);
    public Task<ServiceResult<bool>> RemovePhotographerAssignmentAsync(int eventId, Guid userId);
    public Task<ServiceResult<List<PhotographerAssignment>>> GetAssignmentsForEvent(int eventId);
}
