namespace EventFoto.Data.DTOs;

public record BulkPhotoModifyParams
{
    public IList<int> PhotoIds { get; init; }
}
