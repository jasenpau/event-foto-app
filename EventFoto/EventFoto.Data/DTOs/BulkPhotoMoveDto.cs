namespace EventFoto.Data.DTOs;

public record BulkPhotoMoveDto : BulkPhotoModifyDto
{
    public int DestinationGalleryId { get; init; }
}
