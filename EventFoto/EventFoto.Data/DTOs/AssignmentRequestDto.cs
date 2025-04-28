namespace EventFoto.Data.DTOs;

public record AssignmentRequestDto
{
    public Guid UserId { get; init; }
    public int GalleryId { get; init; }
}
