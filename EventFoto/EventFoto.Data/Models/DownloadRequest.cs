namespace EventFoto.Data.Models;

public record DownloadRequest
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime? ProcessedOn { get; set; }
    public bool IsReady { get; set; }
    public string Filename { get; set; }
    public bool DownloadProcessedPhotos { get; set; }

    public User User { get; init; }
    public IList<DownloadImage> DownloadImages { get; init; }
}
