namespace EventFoto.Data.Models;

public record DownloadImage
{
    public int Id { get; set; }
    public int DownloadRequestId { get; set; }
    public int EventPhotoId { get; set; }

    public DownloadRequest DownloadRequest { get; set; }
    public EventPhoto EventPhoto { get; init; }
}
