namespace EventFoto.Data.Models;

public record ProcessingMessage
{
    public int EventId { get; set; }
    public string Filename { get; set; }
    public DateTime EnqueuedOn { get; set; }
}
