namespace EventFoto.Data.DTOs;

public record BulkPhotoDownloadDto : BulkPhotoModifyDto
{
    public bool Processed { get; init; } = true;
    public int? Quality { get; init; } = null;
}
