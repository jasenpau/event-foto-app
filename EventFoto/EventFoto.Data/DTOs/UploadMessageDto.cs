namespace EventFoto.Data.DTOs;

public record UploadMessageDto
{
    public int EventId { get; set; }
    public string Filename { get; set; }
    public DateTime CaptureDate { get; set; }
}
