namespace EventFoto.Data.Models;

public record UploadBatch
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime? ProcessedOn { get; set; }
    public bool IsReady { get; set; }

    public User User { get; set; }
    public IList<EventPhoto> EventPhotos { get; set; }
}
