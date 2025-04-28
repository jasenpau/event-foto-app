using EventFoto.Data.Models;

namespace EventFoto.Data.DTOs;

public record AssignmentResponseDto
{
    public Guid UserId { get; init; }
    public string UserName { get; init; }
    public int GalleryId { get; init; }
    public string GalleryName { get; init; }

    public static AssignmentResponseDto FromModel(PhotographerAssignment assignment) => new()
    {
        UserId = assignment.UserId,
        UserName = assignment.User?.Name ?? string.Empty,
        GalleryId = assignment.GalleryId,
        GalleryName = assignment.Gallery?.Name ?? string.Empty
    };
}
