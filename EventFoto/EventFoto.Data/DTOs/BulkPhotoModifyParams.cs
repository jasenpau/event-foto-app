namespace EventFoto.Data.DTOs;

public record BulkPhotoModifyParams
{
    public string Action { get; init; }
    public IList<int> PhotoIds { get; init; }
}
