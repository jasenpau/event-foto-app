namespace EventFoto.Data.DTOs;

public record BulkPhotoMoveDto : BulkPhotoModifyDto
{
    public int TargetGalleryId { get; init; }
}
