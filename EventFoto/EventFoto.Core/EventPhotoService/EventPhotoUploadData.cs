using Microsoft.AspNetCore.Http;

namespace EventFoto.Core.EventPhotoService;

public record EventPhotoUploadData
{
    public required IFormFile ImageFile { get; set; }
    public int EventId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CaptureDate { get; set; }
}
