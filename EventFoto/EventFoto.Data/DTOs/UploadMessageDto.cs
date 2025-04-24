namespace EventFoto.Data.DTOs;

public record UploadMessageDto
{
    public int EventId { get; init; }
    public int? GalleryId { get; init; }
    public IList<string> PhotoFilenames { get; init; }
    public DateTime CaptureDate { get; init; }
}
