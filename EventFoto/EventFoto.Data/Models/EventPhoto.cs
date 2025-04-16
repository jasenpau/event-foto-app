namespace EventFoto.Data.Models;

public record EventPhoto
{
    public int Id { get; set; }
    public string Filename { get; set; }
    public DateTime UploadDate { get; set; }
    public DateTime CaptureDate { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsProcessed { get; set; }
    public string ProcessedFilename { get; set; }
    public int EventId { get; set; }
    public Guid UserId { get; set; }

    public User User { get; set; }
    public Event Event { get; set; }
}
