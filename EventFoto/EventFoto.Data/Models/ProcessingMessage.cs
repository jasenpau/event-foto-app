using EventFoto.Data.Enums;

namespace EventFoto.Data.Models;

public record ProcessingMessage
{
    public ProcessingMessageType Type { get; set; }
    public int EntityId { get; set; }
    public string Filename { get; set; }
    public DateTime EnqueuedOn { get; set; }
}
