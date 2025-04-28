namespace EventFoto.Data.DTOs;

public record BulkPhotoDownloadDto : BulkPhotoModifyDto
{
    public bool Processed { get; init; } = true;
}
