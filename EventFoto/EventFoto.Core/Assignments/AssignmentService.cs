using System.Net;
using EventFoto.Data.Models;
using EventFoto.Data.Repositories;

namespace EventFoto.Core.Assignments;

public class AssignmentService : IAssignmentService
{
    private readonly IGalleryRepository _galleryRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IPhotographerAssignmentRepository _assignmentRepository;
    private readonly IUserRepository _userRepository;

    public AssignmentService(IGalleryRepository galleryRepository,
        IEventRepository eventRepository,
        IPhotographerAssignmentRepository assignmentRepository,
        IUserRepository userRepository)
    {
        _galleryRepository = galleryRepository;
        _eventRepository = eventRepository;
        _assignmentRepository = assignmentRepository;
        _userRepository = userRepository;
    }

    public async Task<ServiceResult<PhotographerAssignment>> GetUserAssignmentAsync(int eventId, Guid userId)
    {
        var assignment = await _assignmentRepository.GetForEventAsync(eventId, userId);
        if (assignment == null)
            return ServiceResult<PhotographerAssignment>.Fail("User is not assigned to this event", "not-assigned",
                HttpStatusCode.NotFound);

        return ServiceResult<PhotographerAssignment>.Ok(assignment);
    }

    public async Task<ServiceResult<bool>> AssignPhotographerAsync(int galleryId, Guid userId)
    {
        var gallery = await _galleryRepository.GetByIdAsync(galleryId);
        if (gallery == null)
            return ServiceResult<bool>.Fail("Gallery not found", HttpStatusCode.NotFound);

        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
            return ServiceResult<bool>.Fail("User not found", HttpStatusCode.NotFound);

        var eventAssignment = await _assignmentRepository.GetForEventAsync(gallery.EventId, userId);
        if (eventAssignment != null)
        {
            await RemovePhotographerAssignmentAsync(gallery.EventId, userId);
        }

        var assignment = new PhotographerAssignment
        {
            GalleryId = galleryId,
            UserId = userId
        };
        var result = await _assignmentRepository.AssignPhotographerAsync(assignment);
        return result
            ? ServiceResult<bool>.Ok(true)
            : ServiceResult<bool>.Fail("Failed to assign photographer", HttpStatusCode.InternalServerError);
    }

    public async Task<ServiceResult<bool>> RemovePhotographerAssignmentAsync(int eventId, Guid userId)
    {
        var eventData = await _eventRepository.GetByIdAsync(eventId);
        if (eventData == null)
            return ServiceResult<bool>.Fail("Event not found", HttpStatusCode.NotFound);

        var assignment = await _assignmentRepository.GetForEventAsync(eventId, userId);
        if (assignment == null)
            return ServiceResult<bool>.Fail("User is not assigned to this event", HttpStatusCode.NotFound);


        var result = await _assignmentRepository.RemoveAssignmentAsync(assignment);
        return result
            ? ServiceResult<bool>.Ok(true)
            : ServiceResult<bool>.Fail("Failed to remove photographer assignment", HttpStatusCode.InternalServerError);
    }

    public async Task<ServiceResult<List<PhotographerAssignment>>> GetAssignmentsForEvent(int eventId)
    {
        var eventData = await _eventRepository.GetByIdAsync(eventId);
        if (eventData == null)
            return ServiceResult<List<PhotographerAssignment>>.Fail("Event not found", HttpStatusCode.NotFound);

        var assignments = await _assignmentRepository.GetAssignmentsForEventAsync(eventId);
        return ServiceResult<List<PhotographerAssignment>>.Ok(assignments);
    }
}
