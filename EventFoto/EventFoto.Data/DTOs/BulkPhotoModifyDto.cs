namespace EventFoto.Data.DTOs;

public record BulkPhotoModifyDto
{
    public IList<int> PhotoIds { get; init; }
}
