namespace EventFoto.Data.Models;

public record EventPhoto
{
    public Guid Id { get; init; }
    public string Filename { get; init; }
    public DateTime UploadDate { get; init; }
    public DateTime CaptureDate { get; init; }
    public bool IsDeleted { get; init; }
    public int EventId { get; init; }
    public Guid UserId { get; init; }

    public User User { get; init; }
    public Event Event { get; init; }
}
